using PrestamosApp.Application.Clients.Repositories;
using PrestamosApp.Application.Common.Interfaces;
using PrestamosApp.Application.Installments.Repositories;
using PrestamosApp.Application.Loans.DTOs;
using PrestamosApp.Application.Loans.Repositories;
using PrestamosApp.Application.Payments.Repositories;
using PrestamosApp.Domain.Entities;

namespace PrestamosApp.Application.Loans.Services;

public class LoanService
{
    private readonly ILoanRepository _loanRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IInstallmentRepository _installmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LoanService(
        ILoanRepository loanRepository,
        IClientRepository clientRepository,
        IPaymentRepository paymentRepository,
        IInstallmentRepository installmentRepository,
        IUnitOfWork unitOfWork)
    {
        _loanRepository = loanRepository;
        _clientRepository = clientRepository;
        _paymentRepository = paymentRepository;
        _installmentRepository = installmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<LoanResponseDto>> GetAllAsync()
    {
        var loans = await _loanRepository.GetAllAsync();
        var responses = new List<LoanResponseDto>();

        foreach (var loan in loans)
        {
            responses.Add(await ToResponseAsync(loan));
        }

        return responses;
    }

    public async Task<LoanResponseDto?> GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException(
                "El ID del préstamo debe ser mayor que cero.");
        }

        var loan = await _loanRepository.GetByIdAsync(id);

        return loan is null ? null : await ToResponseAsync(loan);
    }

    public async Task<List<LoanResponseDto>> GetByClientIdAsync(int clientId)
    {
        if (clientId <= 0)
        {
            throw new ArgumentException(
                "El ID del cliente debe ser mayor que cero.");
        }

        var client = await _clientRepository.GetByIdAsync(clientId);

        if (client is null)
        {
            throw new InvalidOperationException(
                "No existe un cliente con el ID indicado.");
        }

        var loans = await _loanRepository.GetByClientIdAsync(clientId);
        var responses = new List<LoanResponseDto>();

        foreach (var loan in loans)
        {
            responses.Add(await ToResponseAsync(loan));
        }

        return responses;
    }

    public async Task<LoanResponseDto> CreateAsync(CreateLoanDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (dto.ClientId <= 0)
        {
            throw new ArgumentException(
                "Debe seleccionar un cliente válido.");
        }

        if (dto.Amount <= 0)
        {
            throw new ArgumentException(
                "El monto debe ser mayor que cero.");
        }

        if (dto.InterestRate < 0 || dto.InterestRate > 100)
        {
            throw new ArgumentException(
                "La tasa de interés debe estar entre 0 y 100.");
        }

        if (dto.TermInMonths < 1 || dto.TermInMonths > 600)
        {
            throw new ArgumentException(
                "El plazo debe estar entre 1 y 600 meses.");
        }

        var client = await _clientRepository.GetByIdAsync(dto.ClientId);

        if (client is null)
        {
            throw new InvalidOperationException(
                "No existe un cliente con el ID indicado.");
        }

        var loan = new Loan
        {
            ClientId = client.Id,
            Amount = decimal.Round(dto.Amount, 2),
            InterestRate = dto.InterestRate,
            TermInMonths = dto.TermInMonths,
            StartDate = DateTime.UtcNow,
            Status = "Pending"
        };

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            loan = await _loanRepository.CreateAsync(loan);

            var installments = GenerateInstallments(loan);

            await _installmentRepository.CreateRangeAsync(installments);

            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }

        return await ToResponseAsync(loan);
    }

    private static List<Installment> GenerateInstallments(Loan loan)
    {
        var monthlyPayment = CalculateMonthlyPayment(loan);
        var installments = new List<Installment>(loan.TermInMonths);
        var accumulated = 0m;

        for (var number = 1; number <= loan.TermInMonths; number++)
        {
            var amountDue = number == loan.TermInMonths
                ? decimal.Round(
                    monthlyPayment * loan.TermInMonths - accumulated, 2)
                : monthlyPayment;

            if (amountDue <= 0)
            {
                throw new InvalidOperationException(
                    "No se pudo calcular correctamente el monto de las cuotas.");
            }

            installments.Add(new Installment
            {
                LoanId = loan.Id,
                InstallmentNumber = number,
                DueDate = loan.StartDate.AddMonths(number),
                AmountDue = amountDue,
                AmountPaid = 0m,
                Status = "Pending",
                PaidAt = null
            });

            accumulated += amountDue;
        }

        return installments;
    }

    private static decimal CalculateMonthlyPayment(Loan loan)
    {
        var monthlyRate = loan.InterestRate / 100m / 12m;

        if (monthlyRate == 0m)
        {
            return decimal.Round(
                loan.Amount / loan.TermInMonths, 2);
        }

        var factor = (decimal)Math.Pow(
            (double)(1m + monthlyRate),
            loan.TermInMonths);

        var payment =
            loan.Amount * monthlyRate * factor / (factor - 1m);

        return decimal.Round(payment, 2);
    }

    private async Task<LoanResponseDto> ToResponseAsync(Loan loan)
    {
        var monthlyPayment = CalculateMonthlyPayment(loan);

        var installments =
            await _installmentRepository.GetByLoanIdAsync(loan.Id);

        var payments =
            await _paymentRepository.GetByLoanIdAsync(loan.Id);

        var totalPayment = installments.Count > 0
            ? installments.Sum(item => item.AmountDue)
            : decimal.Round(monthlyPayment * loan.TermInMonths, 2);

        var totalInterest = decimal.Round(
            totalPayment - loan.Amount, 2);

        var totalPaid = payments.Sum(payment => payment.Amount);
        var outstandingBalance = Math.Max(
            0m, decimal.Round(totalPayment - totalPaid, 2));

        return new LoanResponseDto
        {
            Id = loan.Id,
            ClientId = loan.ClientId,
            ClientName = $"{loan.Client.FirstName} {loan.Client.LastName}",
            Amount = loan.Amount,
            InterestRate = loan.InterestRate,
            TermInMonths = loan.TermInMonths,
            StartDate = loan.StartDate,
            Status = loan.Status,
            MonthlyPayment = monthlyPayment,
            TotalPayment = totalPayment,
            TotalInterest = totalInterest,
            TotalPaid = totalPaid,
            OutstandingBalance = outstandingBalance
        };
    }
}
