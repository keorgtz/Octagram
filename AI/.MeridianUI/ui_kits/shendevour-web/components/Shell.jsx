/* eslint-disable */
// ─────────────────────────────────────────────────────────────
// Shell — Titlebar, Sidebar, Statusbar
// ─────────────────────────────────────────────────────────────

function Titlebar({ crumb, view }) {
  return (
    <header className="titlebar">
      <div className="tb-left">
        <div className="tb-logo">
          <div className="tb-logo-icon"><img src="assets/keorsoft-logo.png" alt="Keorsoft" /></div>
          <div className="tb-logo-name">SH<b>Endevour</b></div>
        </div>
        <nav className="tb-crumb">
          <span>{crumb || "Hotel Misión GDL"}</span>
          <span className="sep">›</span>
          <span className="cur">{view}</span>
        </nav>
      </div>
      <div className="tb-right">
        <div className="tb-chip"><div className="dot"></div>Turno Matutino</div>
        <div className="tb-chip"><MIcon name="notifications_none" />3</div>
        <button className="tb-icon-btn" title="Modo oscuro"><MIcon name="dark_mode" /></button>
        <button className="tb-icon-btn" title="Ayuda"><MIcon name="help_outline" /></button>
        <div className="tb-avatar">JR</div>
      </div>
    </header>
  );
}

const NAV_GROUPS = [
  { items: [
    { id: "dashboard", icon: "dashboard", label: "Dashboard" },
    { id: "metrics", icon: "bar_chart", label: "Métricas del Turno" },
  ] },
  { name: "Operaciones", items: [
    { id: "rooms", icon: "bed", label: "Habitaciones" },
    { id: "checkins", icon: "login", label: "Registros", badge: "8" },
    { id: "reservations", icon: "event", label: "Reservas", badge: "3", badgeKind: "am" },
    { id: "caja", icon: "account_balance_wallet", label: "Caja" },
    { id: "restaurant", icon: "restaurant", label: "Restaurante" },
  ] },
  { name: "Gerencial", items: [
    { id: "reproceso", icon: "refresh", label: "Reproceso" },
    { id: "billing", icon: "receipt_long", label: "Facturación" },
    { id: "reports", icon: "summarize", label: "Reportes" },
  ] },
  { name: "Sistema", items: [
    { id: "settings", icon: "settings", label: "Configuración" },
    { id: "users", icon: "manage_accounts", label: "Usuarios" },
  ] },
];

function Sidebar({ active, onNav }) {
  return (
    <aside className="sidebar">
      <div className="sb-scroll">
        {NAV_GROUPS.map((g, gi) => (
          <React.Fragment key={gi}>
            {gi > 0 && <div className="sb-sep"></div>}
            {g.name && <div className="sb-group">{g.name}</div>}
            {g.items.map((it) => (
              <div
                key={it.id}
                className={"sb-item" + (active === it.id ? " active" : "")}
                onClick={() => onNav && onNav(it.id)}
              >
                <MIcon name={it.icon} />
                <span className="sb-item-name">{it.label}</span>
                {it.badge && <span className={"sb-badge " + (it.badgeKind || "")}>{it.badge}</span>}
              </div>
            ))}
          </React.Fragment>
        ))}
      </div>
      <div className="sb-foot">
        <div className="sb-user">
          <div className="sb-user-av">JR</div>
          <div className="sb-user-info">
            <div className="sb-user-name">J. Rodríguez</div>
            <div className="sb-user-role">Recepcionista · Mat.</div>
          </div>
          <button className="tb-icon-btn" title="Cerrar sesión" style={{ width: 28, height: 28 }}>
            <MIcon name="logout" size={16} />
          </button>
        </div>
      </div>
    </aside>
  );
}

function Statusbar() {
  return (
    <div className="statusbar">
      <div className="sb-l">
        <div className="sb-i"><span className="sb-status-dot"></span>Conectado</div>
        <div className="sb-i">Turno: Matutino (08:00 – 16:00)</div>
        <div className="sb-i">Usuario: J. Rodríguez</div>
      </div>
      <div className="sb-r">
        <div className="sb-i">Hotel Misión GDL</div>
        <div className="sb-i">v2.4.1</div>
        <div className="sb-i">Jue 21 May 2026 · 14:32</div>
      </div>
    </div>
  );
}

function ContentHeader({ title, sub, right }) {
  return (
    <div className="content-header">
      <div className="ch-left">
        <span className="ch-title">{title}</span>
        {sub && <span className="ch-sub">{sub}</span>}
      </div>
      <div className="ch-right">{right}</div>
    </div>
  );
}

Object.assign(window, { Titlebar, Sidebar, Statusbar, ContentHeader });
