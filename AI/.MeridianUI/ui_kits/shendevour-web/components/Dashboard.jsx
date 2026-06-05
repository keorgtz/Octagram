/* eslint-disable */
// ─────────────────────────────────────────────────────────────
// Dashboard view — Resumen del Día
//   4 KPI cards · 4 table cards · 1 caja footer
// ─────────────────────────────────────────────────────────────

const dashStyles = {
  root:    { display: "flex", flexDirection: "column", minHeight: "100%" },
  kpiRow:  { display: "grid", gridTemplateColumns: "repeat(4, 1fr)", padding: "4px 10px" },
  tabRow:  { display: "grid", gridTemplateColumns: "repeat(4, 1fr)", padding: "2px 10px", flex: 1, minHeight: 320 },
  cajaWrap:{ padding: "4px 16px 12px" },
};

const KPI_DATA = [
  {
    label: "LLEGADAS", icon: "flight_land", color: "#10B981",
    number: 17, segs: [
      { val: 6,  color: "#10B981", legend: "Directos" },
      { val: 8,  color: "#34D399", legend: "Reserva" },
      { val: 3,  color: "#6EE7B7", legend: "Probables" },
    ],
  },
  {
    label: "SALIDAS", icon: "flight_takeoff", color: "#F59E0B",
    number: 12, segs: [
      { val: 7,  color: "#F59E0B", legend: "Realizadas" },
      { val: 2,  color: "#FBB95A", legend: "Inesperadas" },
      { val: 3,  color: "#FDE68A", legend: "Programadas" },
    ],
  },
  {
    label: "OCUPACIÓN", icon: "bed", color: "#6366F1",
    number: 86, suffix: "%", badge: { text: "+12%", bg: "#E0E7FF", color: "#6366F1" },
    segs: [
      { val: 78, color: "#6366F1", legend: "Ocupadas" },
      { val: 4,  color: "#A5B4FC", legend: "Uso casa" },
      { val: 4,  color: "#EC4899", legend: "Bloqueadas" },
      { val: 14, color: "#E5E7EB", legend: "Libres" },
    ],
  },
  {
    label: "FORECAST", icon: "pending_actions", color: "#8B5CF6",
    number: 94, suffix: "%", badge: { text: "94%", bg: "#EDE9FE", color: "#8B5CF6" },
    segs: [
      { val: 86, color: "#8B5CF6", legend: "Prob. ocupadas" },
      { val: 4,  color: "#C4B5FD", legend: "Uso casa" },
      { val: 4,  color: "#EC4899", legend: "Bloqueadas" },
      { val: 6,  color: "#E5E7EB", legend: "Libres" },
    ],
  },
];

function KpiCard({ data }) {
  const totalSeg = data.segs.reduce((a, s) => a + s.val, 0) || 1;
  return (
    <div style={{ background: "#fff", borderRadius: 20, margin: 6, boxShadow: "var(--shadow)", overflow: "hidden", position: "relative" }}>
      <div style={{ height: 4, background: data.color }}></div>
      <div style={{ padding: "12px 18px 16px" }}>
        <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", marginBottom: 4 }}>
          <span style={{ fontSize: 11, fontWeight: 600, color: "var(--gray-muted)", textTransform: "uppercase", letterSpacing: ".06em" }}>{data.label}</span>
          <MIcon name={data.icon} size={22} color={data.color} />
        </div>
        <div style={{ display: "flex", alignItems: "flex-end", gap: 10, margin: "4px 0 0" }}>
          <span style={{ fontSize: 42, fontWeight: 700, lineHeight: 1, color: data.color }}>
            {data.number}{data.suffix}
          </span>
          {data.badge && (
            <span style={{ borderRadius: 10, padding: "3px 9px", fontSize: 13, fontWeight: 700, background: data.badge.bg, color: data.badge.color, marginBottom: 3 }}>
              {data.badge.text}
            </span>
          )}
        </div>
        <div style={{ display: "flex", height: 10, borderRadius: 6, overflow: "hidden", margin: "12px 0 10px", gap: 1 }}>
          {data.segs.map((s, i) => (
            <span key={i} style={{
              flex: s.val / totalSeg,
              background: s.color,
              borderTopLeftRadius:  i === 0 ? 5 : 0,
              borderBottomLeftRadius:  i === 0 ? 5 : 0,
              borderTopRightRadius: i === data.segs.length - 1 ? 5 : 0,
              borderBottomRightRadius: i === data.segs.length - 1 ? 5 : 0,
            }}></span>
          ))}
        </div>
        <div style={{ display: "flex", flexWrap: "wrap", gap: "6px 12px" }}>
          {data.segs.map((s, i) => (
            <span key={i} style={{ display: "flex", alignItems: "center", gap: 5, fontSize: 11.5, color: "var(--gray-muted)" }}>
              <span style={{ width: 9, height: 9, borderRadius: "50%", background: s.color }}></span>
              {s.legend} {s.val}
            </span>
          ))}
        </div>
      </div>
    </div>
  );
}

function TableCard({ color, bgPale, bg, label, sub, count, columns, rows, totalLabel, total }) {
  return (
    <div style={{ background: "#fff", borderRadius: 14, margin: 6, boxShadow: "var(--shadow)", display: "flex", flexDirection: "column", overflow: "hidden", minHeight: 0 }}>
      <div style={{ padding: "10px 14px", background: bg, display: "flex", alignItems: "center", justifyContent: "space-between" }}>
        <div>
          <div style={{ fontSize: 10.5, fontWeight: 600, color: "var(--gray-muted)", textTransform: "uppercase", letterSpacing: ".06em" }}>{label}</div>
          <div style={{ fontSize: 12, color, fontWeight: 500, marginTop: 2 }}>{sub}</div>
        </div>
        <div style={{ background: color, color: "#fff", padding: "3px 10px", borderRadius: 10, fontSize: 11, fontWeight: 700 }}>{count}</div>
      </div>
      <div style={{ flex: 1, overflow: "auto" }}>
        <table style={{ width: "100%", borderCollapse: "collapse", fontSize: 12.5 }}>
          <thead>
            <tr>{columns.map((c, i) => <th key={i} style={{ position: "sticky", top: 0, background: "#fff", padding: "7px 10px", textAlign: c.right ? "right" : (c.center ? "center" : "left"), fontSize: 10.5, fontWeight: 600, color: "var(--gray-muted)", borderBottom: "1px solid var(--gray-line)", textTransform: "uppercase", letterSpacing: ".05em" }}>{c.label}</th>)}</tr>
          </thead>
          <tbody>
            {rows.map((r, i) => (
              <tr key={i} style={{ borderBottom: ".5px solid var(--gray-line)" }} onMouseEnter={(e) => e.currentTarget.style.background = "#FAFAFA"} onMouseLeave={(e) => e.currentTarget.style.background = ""}>
                {r.map((cell, j) => <td key={j} style={{ padding: "6px 10px", textAlign: columns[j].right ? "right" : (columns[j].center ? "center" : "left"), fontWeight: columns[j].right ? 600 : 400, color: "#374151", fontVariantNumeric: columns[j].right ? "tabular-nums" : "normal" }}>{cell}</td>)}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      <div style={{ display: "flex", justifyContent: "space-between", padding: "8px 14px", background: "var(--gray-foot)" }}>
        <span style={{ fontSize: 11, color: "var(--gray-muted)" }}>{totalLabel}</span>
        <span style={{ fontSize: 13, fontWeight: 700, color, fontVariantNumeric: "tabular-nums" }}>{money(total)}</span>
      </div>
    </div>
  );
}

const TABLES = [
  {
    color: "#10B981", bg: "#F0FDF4", label: "REGISTROS DEL TURNO", sub: "habitaciones ocupadas hoy", count: 6,
    columns: [{label:"Hab.", center:true},{label:"Huésped"},{label:"Tarifa", right:true}],
    rows: [
      ["318", "García L., Luis A.", money(2400)],
      ["112", "Robles, Ana", money(1800)],
      ["206", "Méndez, Roberto", money(2100)],
      ["404", "Hernández, María", money(1950)],
      ["210", "Torres, Patricia", money(1800)],
      ["301", "Villanueva, José", money(2100)],
    ],
    totalLabel: "Total tarifas", total: 12150,
  },
  {
    color: "#F59E0B", bg: "#FFFBEB", label: "RENTAS / EXTRAS", sub: "movs. de habitación", count: 4,
    columns: [{label:"Cuenta"},{label:"Movs.", center:true},{label:"Total", right:true}],
    rows: [
      ["318 · García L.", "2", money(4400)],
      ["404 · Hernández", "1", money(1950)],
      ["210 · Torres", "1", money(1800)],
      ["112 · Robles", "1", money(1240)],
    ],
    totalLabel: "Total rentas", total: 9390,
  },
  {
    color: "#6366F1", bg: "#EEF2FF", label: "CARGOS", sub: "consumos y servicios", count: 5,
    columns: [{label:"Cuenta"},{label:"Movs.", center:true},{label:"Total", right:true}],
    rows: [
      ["318 · García L.", "3", money(820)],
      ["206 · Méndez", "2", money(620)],
      ["112 · Robles", "1", money(180)],
      ["301 · Villanueva", "2", money(540)],
      ["210 · Torres", "1", money(220)],
    ],
    totalLabel: "Total cargos", total: 2380,
  },
  {
    color: "#8B5CF6", bg: "#F5F3FF", label: "ABONOS", sub: "pagos recibidos", count: 5,
    columns: [{label:"Cuenta"},{label:"Movs.", center:true},{label:"Total", right:true}],
    rows: [
      ["318 · García L.", "1", money(2400)],
      ["404 · Hernández", "1", money(1950)],
      ["112 · Robles", "1", money(1800)],
      ["206 · Méndez", "1", money(2100)],
      ["301 · Villanueva", "1", money(2100)],
    ],
    totalLabel: "Total abonos", total: 10350,
  },
];

function CajaFooter() {
  const items = [
    { icon: "payments", lbl: "Efectivo", val: 18420, color: "#10B981", bg: "#DCFCE7" },
    { icon: "credit_card", lbl: "Tarjetas y otros", val: 26310, color: "#6366F1", bg: "#E0E7FF" },
    { icon: "shopping_bag", lbl: "Gastos / Retiros", val: -1240, color: "#F97316", bg: "#FFEDD5" },
  ];
  const total = 18420 + 26310 - 1240;
  return (
    <div style={{ background: "#fff", borderRadius: 18, boxShadow: "var(--shadow)", display: "flex", alignItems: "center", padding: "6px 8px" }}>
      {items.map((it, i) => (
        <div key={i} style={{ flex: 1, display: "flex", alignItems: "center", gap: 12, padding: "10px 16px" }}>
          <div style={{ background: it.bg, borderRadius: 10, padding: 10, display: "flex" }}>
            <MIcon name={it.icon} size={20} color={it.color} />
          </div>
          <div>
            <div style={{ fontSize: 10, fontWeight: 600, color: "var(--gray-muted)", textTransform: "uppercase", letterSpacing: ".06em" }}>{it.lbl}</div>
            <div style={{ fontSize: 18, fontWeight: 700, color: it.color, marginTop: 1, fontVariantNumeric: "tabular-nums" }}>{moneyShort(it.val)}</div>
          </div>
        </div>
      ))}
      <div style={{ width: 1, background: "var(--gray-line)", height: 44 }}></div>
      <div style={{ flex: 1, display: "flex", alignItems: "center", gap: 12, padding: "10px 16px" }}>
        <div style={{ background: "#EDE9FE", borderRadius: 10, padding: 10, display: "flex" }}>
          <MIcon name="account_balance_wallet" size={20} color="#8B5CF6" />
        </div>
        <div>
          <div style={{ fontSize: 10, fontWeight: 600, color: "var(--gray-muted)", textTransform: "uppercase", letterSpacing: ".06em" }}>Total caja</div>
          <div style={{ fontSize: 22, fontWeight: 700, color: "#8B5CF6", marginTop: 1, fontVariantNumeric: "tabular-nums" }}>{moneyShort(total)}</div>
        </div>
      </div>
    </div>
  );
}

function DashboardView({ isLoading, onRefresh }) {
  return (
    <React.Fragment>
      <ContentHeader
        title="Resumen del Día"
        sub="jueves, 21 de mayo de 2026"
        right={
          <React.Fragment>
            {isLoading && <span style={{ width: 22, height: 22, border: "3px solid rgba(139,92,246,.2)", borderTopColor: "#8B5CF6", borderRadius: "50%", animation: "spin .8s linear infinite", display: "inline-block" }}></span>}
            <button className="btn-refresh" onClick={onRefresh}><MIcon name="refresh" />Actualizar</button>
          </React.Fragment>
        }
      />
      <div className="main-scroll">
        <div style={dashStyles.root}>
          <div style={dashStyles.kpiRow}>
            {KPI_DATA.map((k, i) => <KpiCard key={i} data={k} />)}
          </div>
          <div style={dashStyles.tabRow}>
            {TABLES.map((t, i) => <TableCard key={i} {...t} />)}
          </div>
          <div style={dashStyles.cajaWrap}>
            <CajaFooter />
          </div>
        </div>
      </div>
    </React.Fragment>
  );
}

Object.assign(window, { DashboardView });
