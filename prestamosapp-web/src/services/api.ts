const API_URL = (import.meta.env.VITE_API_URL || "http://localhost:5190/api").replace(/\/$/, "");

export interface Client {
  id: number;
  firstName: string;
  lastName: string;
  identificationNumber: string;
  phone: string;
  address: string;
  createdAt: string;
}

export interface Loan {
  id: number;
  clientId: number;
  clientName: string;
  amount: number;
  interestRate: number;
  termInMonths: number;
  startDate: string;
  status: string;
  monthlyPayment: number;
  totalPayment: number;
  totalInterest: number;
  totalPaid: number;
  outstandingBalance: number;
}

async function getList<T>(endpoint: string): Promise<T[]> {
  const response = await fetch(`${API_URL}${endpoint}`);

  if (!response.ok) {
    throw new Error(
      `Error ${response.status} consultando ${endpoint}`
    );
  }

  const result: unknown = await response.json();

  // La API puede responder directamente con una lista.
  if (Array.isArray(result)) {
    return result as T[];
  }

  // También admite una lista dentro de value, Value, data o items.
  if (result !== null && typeof result === "object") {
    const data = result as Record<string, unknown>;

    const list =
      data["value"] ??
      data["Value"] ??
      data["data"] ??
      data["Data"] ??
      data["items"] ??
      data["Items"];

    if (Array.isArray(list)) {
      return list as T[];
    }

    throw new Error(
      `Respuesta inválida de ${endpoint}. Propiedades recibidas: ${
        Object.keys(data).join(", ") || "ninguna"
      }`
    );
  }

  throw new Error(
    `La respuesta de ${endpoint} no contiene una lista válida.`
  );
}

export function getClients(): Promise<Client[]> {
  return getList<Client>("/clients");
}

export function getLoans(): Promise<Loan[]> {
  return getList<Loan>("/loans");
}

export function formatCurrency(amount: number): string {
  return new Intl.NumberFormat("es-DO", {
    style: "currency",
    currency: "DOP",
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(amount);
}

