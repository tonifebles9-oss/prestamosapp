using PrestamosApp.Application.Common.Interfaces;
using PrestamosApp.Application.Installments.Repositories;
using PrestamosApp.Application.Loans.Repositories;
using PrestamosApp.Application.Payments.DTOs;
using PrestamosApp.Application.Payments.Repositories;
using PrestamosApp.Domain.Entities;

namespace PrestamosApp.Application.Payments.Services;

public class PaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILoanRepository _loanRepository;
    private readonly IInstallmentRepository _installmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentService(
        IPaymentRepository paymentRepository,
        ILoanRepository loanRepository,
        IInstallmentRepository installmentRepository,
        IUnitOfWork unitOfWork)
    {
        _paymentRepository = paymentRepository;
        _loanRepository = loanRepository;
        _installmentRepository = installmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<PaymentResponseDto>> GetByLoanIdAsync(
        int loanId)
    {
        if (loanId <= 0)
        {
            throw new ArgumentException(
                "El préstamo seleccionado no es válido.");
        }

        var loan = await _loanRepository.GetByIdAsync(loanId);

        if (loan is null)
        {
            throw new KeyNotFoundException(
                "No se encontró el préstamo.");
        }

        var payments =
            await _paymentRepository.GetByLoanIdAsync(loanId);

        var installments =
            await _installmentRepository.GetByLoanIdAsync(loanId);

        var totalDue = installments.Count > 0
            ? installments.Sum(item => item.AmountDue)
            : CalculateTotalDue(loan);

        var totalPaid = payments.Sum(payment => payment.Amount);

        var outstandingBalance = Math.Max(
            0m,
            totalDue - totalPaid);

        return payments.Select(payment => new PaymentResponseDto
        {
            Id = payment.Id,
            LoanId = payment.LoanId,
            InstallmentNumber = payment.InstallmentNumber,
            Amount = payment.Amount,
            PaymentDate = payment.PaymentDate,
            PaymentMethod = payment.PaymentMethod,
            Notes = payment.Notes,
            TotalPaid = totalPaid,
            OutstandingBalance = outstandingBalance
        }).ToList();
    }

    public async Task<PaymentResponseDto> CreateAsync(
        CreatePaymentDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (dto.LoanId <= 0)
        {
            throw new ArgumentException(
                "Debe seleccionar un préstamo válido.");
        }

        var paymentAmount = decimal.Round(dto.Amount, 2);

        if (paymentAmount <= 0m)
        {
            throw new ArgumentException(
                "El monto del pago debe ser mayor que cero.");
        }

        if (string.IsNullOrWhiteSpace(dto.PaymentMethod))
        {
            throw new ArgumentException(
                "El método de pago es obligatorio.");
        }

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var loan =
                await _loanRepository.GetByIdAsync(dto.LoanId);

            if (loan is null)
            {
                throw new KeyNotFoundException(
                    "No se encontró el préstamo.");
            }

            var installments =
                await _installmentRepository.GetByLoanIdAsync(
                    dto.LoanId);

            if (installments.Count == 0)
            {
                throw new InvalidOperationException(
                    "El préstamo no tiene cuotas generadas.");
            }

            var totalDue = installments.Sum(
                installment => installment.AmountDue);

            var existingPayments =
                await _paymentRepository.GetByLoanIdAsync(
                    dto.LoanId);

            var totalPaid = existingPayments.Sum(
                payment => payment.Amount);

            var outstandingBalance = Math.Max(
                0m,
                totalDue - totalPaid);

            if (outstandingBalance <= 0m)
            {
                throw new InvalidOperationException(
                    "Este préstamo ya está completamente pagado.");
            }

            if (paymentAmount > outstandingBalance)
            {
                throw new ArgumentException(
                    $"El pago supera el saldo pendiente de " +
                    $"{outstandingBalance:N2}.");
            }

            var remainingPayment = paymentAmount;

            var pendingInstallments = installments
                .Where(installment =>
                    installment.AmountPaid < installment.AmountDue)
                .OrderBy(installment =>
                    installment.InstallmentNumber)
                .ToList();

            if (pendingInstallments.Count == 0)
            {
                throw new InvalidOperationException(
                    "No existen cuotas pendientes para este préstamo.");
            }

            var affectedInstallmentNumber =
                pendingInstallments[0].InstallmentNumber;

            foreach (var installment in pendingInstallments)
            {
                if (remainingPayment <= 0m)
                {
                    break;
                }

                var installmentBalance = decimal.Round(
                    installment.AmountDue - installment.AmountPaid,
                    2);

                if (installmentBalance <= 0m)
                {
                    continue;
                }

                var amountToApply = Math.Min(
                    remainingPayment,
                    installmentBalance);

                installment.AmountPaid = decimal.Round(
                    installment.AmountPaid + amountToApply,
                    2);

                remainingPayment = decimal.Round(
                    remainingPayment - amountToApply,
                    2);

                if (installment.AmountPaid >= installment.AmountDue)
                {
                    installment.AmountPaid = installment.AmountDue;
                    installment.Status = "Paid";
                    installment.PaidAt = DateTime.UtcNow;
                }
                else
                {
                    installment.Status = "PartiallyPaid";
                    installment.PaidAt = null;
                }
            }

            if (remainingPayment > 0m)
            {
                throw new InvalidOperationException(
                    "No fue posible aplicar todo el importe del pago.");
            }

            await _installmentRepository.UpdateRangeAsync(
                installments);

            var payment = new Payment
            {
                LoanId = dto.LoanId,
                InstallmentNumber = affectedInstallmentNumber,
                Amount = paymentAmount,
                PaymentDate = DateTime.UtcNow,
                PaymentMethod = dto.PaymentMethod.Trim(),
                Notes = dto.Notes?.Trim() ?? string.Empty
            };

            var savedPayment =
                await _paymentRepository.CreateAsync(payment);

            totalPaid += savedPayment.Amount;

            outstandingBalance = Math.Max(
                0m,
                totalDue - totalPaid);

            var newStatus = outstandingBalance == 0m
                ? "Paid"
                : "PartiallyPaid";

            await _loanRepository.UpdateStatusAsync(
                dto.LoanId,
                newStatus);

            await _unitOfWork.CommitTransactionAsync();

            return new PaymentResponseDto
            {
                Id = savedPayment.Id,
                LoanId = savedPayment.LoanId,
                InstallmentNumber = savedPayment.InstallmentNumber,
                Amount = savedPayment.Amount,
                PaymentDate = savedPayment.PaymentDate,
                PaymentMethod = savedPayment.PaymentMethod,
                Notes = savedPayment.Notes,
                TotalPaid = totalPaid,
                OutstandingBalance = outstandingBalance
            };
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    private static decimal CalculateTotalDue(Loan loan)
    {
        if (loan.Amount <= 0m ||
            loan.TermInMonths <= 0 ||
            loan.InterestRate < 0m)
        {
            throw new InvalidOperationException(
                "El préstamo tiene datos financieros inválidos.");
        }

        var monthlyRate = loan.InterestRate / 100m / 12m;

        decimal monthlyPayment;

        if (monthlyRate == 0m)
        {
            monthlyPayment =
                loan.Amount / loan.TermInMonths;
        }
        else
        {
            var factor = (decimal)Math.Pow(
                (double)(1m + monthlyRate),
                loan.TermInMonths);

            monthlyPayment =
                loan.Amount *
                monthlyRate *
                factor /
                (factor - 1m);
        }

        monthlyPayment = decimal.Round(monthlyPayment, 2);

        return decimal.Round(
            monthlyPayment * loan.TermInMonths,
            2);
    }
}
