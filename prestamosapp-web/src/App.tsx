import { useEffect, useMemo, useState } from "react";
import {
  LayoutDashboard,
  Users,
  HandCoins,
  CalendarDays,
  WalletCards,
  Settings,
  Bell,
  Search,
  ArrowUpRight,
  CircleDollarSign,
  Menu,
  X,
  ChevronRight,
  ShieldCheck,
  FileText,
  RefreshCw,
} from "lucide-react";
import "./App.css";
import { getClients, getLoans, formatCurrency } from "./services/api";
import type { Client, Loan } from "./services/api";

const navigation = [
  { label: "Dashboard", icon: LayoutDashboard },
  { label: "Clientes", icon: Users },
  { label: "Préstamos", icon: HandCoins },
  { label: "Cuotas", icon: CalendarDays },
  { label: "Pagos", icon: WalletCards },
  { label: "Reportes", icon: FileText },
];

const descriptions: Record<string, string> = {
  Dashboard: "Resumen general de tu negocio",
  Clientes: "Consulta tus clientes registrados",
  "Préstamos": "Consulta y administra tus préstamos",
  Cuotas: "Controla los vencimientos y las cuotas",
  Pagos: "Consulta los pagos recibidos",
  Reportes: "Resumen de tu cartera de préstamos",
  Configuración: "Preferencias del sistema",
};

function App() {
  const [activePage, setActivePage] = useState("Dashboard");
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [search, setSearch] = useState("");
  const [clients, setClients] = useState<Client[]>([]);
  const [loans, setLoans] = useState<Loan[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  async function loadData() {
    setLoading(true);
    setError("");

    try {
      const [clientsData, loansData] = await Promise.all([
        getClients(),
        getLoans(),
      ]);
      setClients(clientsData);
      setLoans(loansData);
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "No fue posible cargar los datos de la API.",
      );
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void loadData();
  }, []);

  const filteredClients = useMemo(() => {
    const term = search.trim().toLowerCase();
    if (!term) return clients;

    return clients.filter((client) =>
      [
        client.firstName,
        client.lastName,
        client.identificationNumber,
        client.phone,
      ].some((value) => value?.toLowerCase().includes(term)),
    );
  }, [clients, search]);

  const filteredLoans = useMemo(() => {
    const term = search.trim().toLowerCase();
    if (!term) return loans;

    return loans.filter((loan) =>
      [String(loan.id), loan.clientName, loan.status].some((value) =>
        value?.toLowerCase().includes(term),
      ),
    );
  }, [loans, search]);

  const totalPrestado = loans.reduce((sum, loan) => sum + loan.amount, 0);
  const totalCobrado = loans.reduce((sum, loan) => sum + loan.totalPaid, 0);
  const saldoPendiente = loans.reduce(
    (sum, loan) => sum + loan.outstandingBalance,
    0,
  );

  const selectPage = (page: string) => {
    setActivePage(page);
    setSidebarOpen(false);
    setSearch("");
  };

  const statusLabel = (status: string) => {
    const labels: Record<string, string> = {
      Pending: "Pendiente",
      PartiallyPaid: "Pago parcial",
      Paid: "Pagado",
      Active: "Activo",
      Completed: "Completado",
      Cancelled: "Cancelado",
    };
    return labels[status] ?? status;
  };

  const cellStyle = {
    padding: "14px 12px",
    borderBottom: "1px solid #e9edf3",
    fontSize: "14px",
    verticalAlign: "middle" as const,
  };

  const headingStyle = {
    ...cellStyle,
    color: "#64748b",
    fontSize: "12px",
    fontWeight: 600,
    textTransform: "uppercase" as const,
    background: "#f8fafc",
  };

  const tableStyle = {
    width: "100%",
    borderCollapse: "collapse" as const,
    textAlign: "left" as const,
  };

  function renderLoans(items: Loan[]) {
    if (items.length === 0) {
      return (
        <p style={{ padding: 24, color: "#64748b" }}>
          {search
            ? "No se encontraron préstamos con esa búsqueda."
            : "Todavía no hay préstamos registrados."}
        </p>
      );
    }

    return (
      <div style={{ overflowX: "auto" }}>
        <table style={tableStyle}>
          <thead>
            <tr>
              <th style={headingStyle}>Préstamo</th>
              <th style={headingStyle}>Cliente</th>
              <th style={headingStyle}>Monto</th>
              <th style={headingStyle}>Cuota mensual</th>
              <th style={headingStyle}>Pagado</th>
              <th style={headingStyle}>Saldo pendiente</th>
              <th style={headingStyle}>Estado</th>
            </tr>
          </thead>
          <tbody>
            {items.map((loan) => (
              <tr key={loan.id}>
                <td style={cellStyle}>#{loan.id}</td>
                <td style={cellStyle}>
                  <strong>{loan.clientName || `Cliente #${loan.clientId}`}</strong>
                </td>
                <td style={cellStyle}>{formatCurrency(loan.amount)}</td>
                <td style={cellStyle}>{formatCurrency(loan.monthlyPayment)}</td>
                <td style={cellStyle}>{formatCurrency(loan.totalPaid)}</td>
                <td style={cellStyle}>
                  <strong>{formatCurrency(loan.outstandingBalance)}</strong>
                </td>
                <td style={cellStyle}>
                  <span
                    style={{
                      display: "inline-block",
                      padding: "5px 9px",
                      borderRadius: 20,
                      fontSize: 12,
                      background:
                        loan.status === "Paid" || loan.status === "Completed"
                          ? "#dcfce7"
                          : loan.status === "PartiallyPaid"
                            ? "#fef3c7"
                            : "#e2e8f0",
                      color:
                        loan.status === "Paid" || loan.status === "Completed"
                          ? "#166534"
                          : loan.status === "PartiallyPaid"
                            ? "#92400e"
                            : "#334155",
                    }}
                  >
                    {statusLabel(loan.status)}
                  </span>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    );
  }

  return (
    <div className="app-shell">
      {sidebarOpen && (
        <button
          className="mobile-overlay"
          aria-label="Cerrar menú"
          onClick={() => setSidebarOpen(false)}
        />
      )}

      <aside className={`sidebar ${sidebarOpen ? "sidebar-open" : ""}`}>
        <div className="brand">
          <div className="brand-icon">
            <CircleDollarSign size={25} />
          </div>
          <div>
            <h1>PrestamosApp</h1>
            <span>GESTIÓN FINANCIERA</span>
          </div>
          <button
            className="icon-button mobile-close"
            onClick={() => setSidebarOpen(false)}
            aria-label="Cerrar menú"
          >
            <X size={20} />
          </button>
        </div>

        <div className="nav-caption">MENÚ PRINCIPAL</div>
        <nav className="navigation">
          {navigation.map(({ label, icon: Icon }) => (
            <button
              key={label}
              className={`nav-item ${activePage === label ? "nav-active" : ""}`}
              onClick={() => selectPage(label)}
            >
              <Icon size={19} strokeWidth={1.9} />
              <span>{label}</span>
              {activePage === label && <span className="active-dot" />}
            </button>
          ))}
        </nav>

        <div className="sidebar-spacer" />
        <div className="security-card">
          <div className="security-icon">
            <ShieldCheck size={20} />
          </div>
          <strong>Tu negocio, organizado</strong>
          <p>Controla tus préstamos y pagos desde un solo lugar.</p>
        </div>

        <button
          className={`nav-item settings-item ${
            activePage === "Configuración" ? "nav-active" : ""
          }`}
          onClick={() => selectPage("Configuración")}
        >
          <Settings size={19} />
          <span>Configuración</span>
        </button>

        <div className="profile">
          <div className="avatar">PA</div>
          <div className="profile-text">
            <strong>Administrador</strong>
            <span>Panel de gestión</span>
          </div>
          <ChevronRight size={17} className="profile-arrow" />
        </div>
      </aside>

      <main className="main-content">
        <header className="topbar">
          <div className="topbar-left">
            <button
              className="icon-button menu-toggle"
              aria-label="Abrir menú"
              onClick={() => setSidebarOpen(true)}
            >
              <Menu size={22} />
            </button>
            <div className="breadcrumb">
              <span>PrestamosApp</span>
              <ChevronRight size={15} />
              <strong>{activePage}</strong>
            </div>
          </div>

          <div className="topbar-actions">
            <label className="search-box">
              <Search size={18} />
              <input
                value={search}
                onChange={(event) => setSearch(event.target.value)}
                placeholder="Buscar..."
                aria-label="Buscar"
              />
              {search && (
                <button
                  className="search-clear"
                  onClick={() => setSearch("")}
                  aria-label="Limpiar búsqueda"
                >
                  <X size={15} />
                </button>
              )}
            </label>
            <button
              className="icon-button"
              aria-label="Actualizar datos"
              title="Actualizar datos"
              onClick={() => void loadData()}
              disabled={loading}
            >
              <RefreshCw size={18} />
            </button>
            <button
              className="icon-button notification-button"
              aria-label="Notificaciones"
              title="Notificaciones"
            >
              <Bell size={20} />
              <span />
            </button>
            <div className="top-avatar">PA</div>
          </div>
        </header>

        <div className="page-content">
          <section className="welcome-row">
            <div>
              <div className="eyebrow">PANEL DE CONTROL</div>
              <h2>{activePage === "Dashboard" ? "¡Bienvenido!" : activePage}</h2>
              <p>{descriptions[activePage] ?? "Administra tus datos"}</p>
            </div>
            <button
              className="primary-button"
              onClick={() =>
                activePage === "Dashboard"
                  ? selectPage("Préstamos")
                  : void loadData()
              }
            >
              {activePage === "Dashboard" ? (
                <ArrowUpRight size={18} />
              ) : (
                <RefreshCw size={18} />
              )}
              <span>
                {activePage === "Dashboard" ? "Ver préstamos" : "Actualizar datos"}
              </span>
            </button>
          </section>

          {error && (
            <section
              className="panel-card"
              style={{
                padding: 18,
                marginBottom: 20,
                color: "#991b1b",
                background: "#fef2f2",
              }}
            >
              <strong>No se pudieron cargar los datos.</strong>
              <p>{error}</p>
              <p>Comprueba que la API esté funcionando en http://localhost:5190.</p>
              <button className="primary-button" onClick={() => void loadData()}>
                Intentar de nuevo
              </button>
            </section>
          )}

          {loading && (
            <section className="panel-card" style={{ padding: 24 }}>
              Cargando datos reales de PrestamosApp...
            </section>
          )}

          {!loading && !error && activePage === "Dashboard" && (
            <>
              <section className="status-banner">
                <div className="status-symbol">
                  <CircleDollarSign size={21} />
                </div>
                <div className="status-copy">
                  <strong>Datos conectados correctamente</strong>
                  <p>La información se consulta desde tu API de .NET.</p>
                </div>
                <span className="status-pill">
                  <span className="status-dot" />
                  API conectada
                </span>
              </section>

              <section className="stats-grid">
                <article className="stat-card">
                  <div className="stat-top">
                    <span className="stat-label">Total prestado</span>
                    <div className="stat-icon icon-purple">
                      <HandCoins size={20} />
                    </div>
                  </div>
                  <div className="stat-value">{formatCurrency(totalPrestado)}</div>
                  <div className="stat-foot">Capital de los préstamos registrados</div>
                </article>

                <article className="stat-card">
                  <div className="stat-top">
                    <span className="stat-label">Total cobrado</span>
                    <div className="stat-icon icon-green">
                      <WalletCards size={20} />
                    </div>
                  </div>
                  <div className="stat-value">{formatCurrency(totalCobrado)}</div>
                  <div className="stat-foot">Pagos registrados en los préstamos</div>
                </article>

                <article className="stat-card">
                  <div className="stat-top">
                    <span className="stat-label">Saldo pendiente</span>
                    <div className="stat-icon icon-orange">
                      <CircleDollarSign size={20} />
                    </div>
                  </div>
                  <div className="stat-value">{formatCurrency(saldoPendiente)}</div>
                  <div className="stat-foot">Saldo por cobrar según la API</div>
                </article>

                <article className="stat-card">
                  <div className="stat-top">
                    <span className="stat-label">Clientes</span>
                    <div className="stat-icon icon-blue">
                      <Users size={20} />
                    </div>
                  </div>
                  <div className="stat-value">{clients.length}</div>
                  <div className="stat-foot">Clientes registrados</div>
                </article>
              </section>

              <section className="panel-card" style={{ marginBottom: 22 }}>
                <div className="panel-heading" style={{ padding: 20 }}>
                  <div>
                    <h3>Préstamos recientes</h3>
                    <p>{loans.length} préstamos encontrados</p>
                  </div>
                  <button
                    className="subtle-button"
                    onClick={() => selectPage("Préstamos")}
                  >
                    Ver todos <ArrowUpRight size={15} />
                  </button>
                </div>
                {renderLoans(
                  [...loans]
                    .sort(
                      (a, b) =>
                        new Date(b.startDate).getTime() -
                        new Date(a.startDate).getTime(),
                    )
                    .slice(0, 5),
                )}
              </section>

              <section className="panel-card">
                <div className="panel-heading" style={{ padding: 20 }}>
                  <div>
                    <h3>Clientes registrados</h3>
                    <p>Clientes disponibles en el sistema</p>
                  </div>
                  <button
                    className="subtle-button"
                    onClick={() => selectPage("Clientes")}
                  >
                    Ver clientes <ArrowUpRight size={15} />
                  </button>
                </div>
                {clients.length === 0 ? (
                  <p style={{ padding: 20 }}>No hay clientes registrados.</p>
                ) : (
                  <div style={{ overflowX: "auto" }}>
                    <table style={tableStyle}>
                      <thead>
                        <tr>
                          <th style={headingStyle}>Nombre</th>
                          <th style={headingStyle}>Identificación</th>
                          <th style={headingStyle}>Teléfono</th>
                          <th style={headingStyle}>Dirección</th>
                        </tr>
                      </thead>
                      <tbody>
                        {clients.slice(0, 5).map((client) => (
                          <tr key={client.id}>
                            <td style={cellStyle}>
                              {client.firstName} {client.lastName}
                            </td>
                            <td style={cellStyle}>{client.identificationNumber}</td>
                            <td style={cellStyle}>{client.phone}</td>
                            <td style={cellStyle}>{client.address}</td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                )}
              </section>
            </>
          )}

          {!loading && !error && activePage === "Préstamos" && (
            <section className="panel-card">
              <div className="panel-heading" style={{ padding: 20 }}>
                <div>
                  <h3>Todos los préstamos</h3>
                  <p>{filteredLoans.length} resultados</p>
                </div>
              </div>
              {renderLoans(filteredLoans)}
            </section>
          )}

          {!loading && !error && activePage === "Clientes" && (
            <section className="panel-card">
              <div className="panel-heading" style={{ padding: 20 }}>
                <div>
                  <h3>Todos los clientes</h3>
                  <p>{filteredClients.length} resultados</p>
                </div>
              </div>
              {filteredClients.length === 0 ? (
                <p style={{ padding: 24 }}>
                  {search ? "No se encontraron clientes." : "Todavía no hay clientes registrados."}
                </p>
              ) : (
                <div style={{ overflowX: "auto" }}>
                  <table style={tableStyle}>
                    <thead>
                      <tr>
                        <th style={headingStyle}>ID</th>
                        <th style={headingStyle}>Nombre</th>
                        <th style={headingStyle}>Identificación</th>
                        <th style={headingStyle}>Teléfono</th>
                        <th style={headingStyle}>Dirección</th>
                        <th style={headingStyle}>Registro</th>
                      </tr>
                    </thead>
                    <tbody>
                      {filteredClients.map((client) => (
                        <tr key={client.id}>
                          <td style={cellStyle}>{client.id}</td>
                          <td style={cellStyle}>
                            {client.firstName} {client.lastName}
                          </td>
                          <td style={cellStyle}>{client.identificationNumber}</td>
                          <td style={cellStyle}>{client.phone}</td>
                          <td style={cellStyle}>{client.address}</td>
                          <td style={cellStyle}>
                            {new Date(client.createdAt).toLocaleDateString("es-DO")}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </section>
          )}

          {!loading &&
            !error &&
            !["Dashboard", "Clientes", "Préstamos"].includes(activePage) && (
              <section className="panel-card" style={{ padding: 24 }}>
                <div className="placeholder-icon">
                  {activePage === "Cuotas" ? (
                    <CalendarDays size={27} />
                  ) : activePage === "Pagos" ? (
                    <WalletCards size={27} />
                  ) : activePage === "Reportes" ? (
                    <FileText size={27} />
                  ) : (
                    <Settings size={27} />
                  )}
                </div>
                <h3>{activePage}</h3>
                <p>
                  Esta sección todavía no está conectada a sus operaciones
                  específicas. Los datos de clientes y préstamos ya se cargan
                  desde la API.
                </p>
                <button
                  className="primary-button"
                  onClick={() => selectPage("Dashboard")}
                >
                  Volver al dashboard
                </button>
              </section>
            )}

          <footer className="footer">
            <span>© 2026 PrestamosApp</span>
            <span>Gestión de préstamos</span>
          </footer>
        </div>
      </main>
    </div>
  );
}

export default App;
