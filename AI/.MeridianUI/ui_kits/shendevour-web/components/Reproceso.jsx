/* eslint-disable */
// ─────────────────────────────────────────────────────────────
// Reproceso view — clasificación de notas fiscales / no fiscales
// ─────────────────────────────────────────────────────────────

const PAY_MAP = {
  efec:  { kind: "efec",  icon: "payments",     label: "Efectivo" },
  tcred: { kind: "tcred", icon: "credit_card",  label: "T. Crédito" },
  tdeb:  { kind: "tdeb",  icon: "credit_card",  label: "T. Débito" },
  trans: { kind: "trans", icon: "swap_horiz",   label: "Transferencia" },
};

const NOTAS_SEED = [
  { num: 4218, origen: "Recepción · M.", cliente: "García López, Luis Antonio",   total:  4820, pago: "tcred", facturada: true,
    movs: [
      { tipo: "cargo", concepto: "Hospedaje 1 noche",   grupo: "Renta · Hab 318",      ref: "Folio 4218-A", monto:  4200 },
      { tipo: "cargo", concepto: "Servicio a habitación", grupo: "Restaurante",        ref: "Tic-1820",     monto:  1240 },
      { tipo: "abono", concepto: "Depósito reserva",    grupo: "Pago anticipado",      ref: "Trans-918",    monto:  -620 },
    ] },
  { num: 4219, origen: "Recepción · M.", cliente: "Hernández Vega, María José",   total:  1950, pago: "efec",  facturada: false,
    movs: [
      { tipo: "cargo", concepto: "Hospedaje 1 noche",   grupo: "Renta · Hab 404",      ref: "Folio 4219-A", monto:  1950 },
    ] },
  { num: 4220, origen: "Restaurante",    cliente: "Venta al público",              total:   620, pago: "efec",  facturada: false,
    movs: [
      { tipo: "cargo", concepto: "Desayuno buffet x2",  grupo: "Restaurante",          ref: "Tic-1821",     monto:   620 },
    ] },
  { num: 4221, origen: "Recepción · M.", cliente: "Méndez Torres, Roberto",        total:  2960, pago: "tdeb",  facturada: false,
    movs: [
      { tipo: "cargo", concepto: "Hospedaje 1 noche",   grupo: "Renta · Hab 206",      ref: "Folio 4221-A", monto:  2100 },
      { tipo: "cargo", concepto: "Lavandería",          grupo: "Servicios",            ref: "OS-220",       monto:   860 },
    ] },
  { num: 4222, origen: "Recepción · M.", cliente: "Robles Castro, Ana Cristina",   total:  1240, pago: "trans", facturada: true,
    movs: [
      { tipo: "cargo", concepto: "Servicio a habitación", grupo: "Restaurante",        ref: "Tic-1822",     monto:  1240 },
    ] },
  { num: 4223, origen: "Restaurante",    cliente: "Venta al público",              total:   840, pago: "efec",  facturada: false,
    movs: [
      { tipo: "cargo", concepto: "Comida x2",           grupo: "Restaurante",          ref: "Tic-1823",     monto:   840 },
    ] },
];

function NotaRow({ nota, selected, expanded, onSelect, onExpand }) {
  const pay = PAY_MAP[nota.pago];
  return (
    <div style={{ background: "#fff", borderRadius: 8, boxShadow: "var(--shadow)", marginBottom: 6, overflow: "hidden" }}>
      <div style={{ display: "grid", gridTemplateColumns: "24px 70px 140px 1fr 120px 240px 80px", alignItems: "center", gap: 12, padding: "10px 14px" }}>
        <Checkbox checked={selected} disabled={nota.facturada} onChange={onSelect} />
        <div><div style={{ fontSize: 9, fontWeight: 700, color: "var(--gray-muted)", letterSpacing: ".08em" }}>NOTA</div><div style={{ fontSize: 14, fontWeight: 700, color: "var(--gray-dark)" }}>{nota.num}</div></div>
        <div><div style={{ fontSize: 9, fontWeight: 700, color: "var(--gray-muted)", letterSpacing: ".08em" }}>ORIGEN</div><div style={{ fontSize: 11, color: "var(--gray-700)" }}>{nota.origen}</div></div>
        <div style={{ minWidth: 0 }}><div style={{ fontSize: 9, fontWeight: 700, color: "var(--gray-muted)", letterSpacing: ".08em" }}>CLIENTE</div><div style={{ fontSize: 13, color: "var(--gray-dark)", overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap" }}>{nota.cliente}</div></div>
        <div><div style={{ fontSize: 9, fontWeight: 700, color: "var(--gray-muted)", letterSpacing: ".08em" }}>TOTAL NETO</div><div style={{ fontSize: 15, fontWeight: 700, color: "var(--primary)", fontVariantNumeric: "tabular-nums" }}>{money(nota.total)}</div></div>
        <div style={{ display: "flex", gap: 6, justifyContent: "flex-end" }}>
          <Chip kind={pay.kind} icon={pay.icon}>{pay.label}</Chip>
          {nota.facturada && <Chip kind="fact" icon="check_circle">Facturada</Chip>}
        </div>
        <div style={{ display: "flex", alignItems: "center", gap: 4, color: "var(--gray-muted)", fontSize: 12, justifyContent: "flex-end", cursor: "pointer" }} onClick={onExpand}>
          {nota.movs.length} movs
          <MIcon name={expanded ? "expand_less" : "expand_more"} size={18} />
        </div>
      </div>
      {expanded && (
        <div style={{ background: "#e9e9e9", padding: "10px 14px", borderTop: "1px solid var(--gray-line)" }}>
          <div style={{ display: "flex", alignItems: "center", gap: 10, marginBottom: 6 }}>
            <span style={{ fontSize: 9, fontWeight: 700, color: "var(--gray-muted)", letterSpacing: ".08em" }}>MOVIMIENTOS</span>
            <span style={{ padding: "2px 8px", borderRadius: 8, background: "#fff", fontSize: 11, fontWeight: 600, color: "#1565C0" }}>Cargos: {money(nota.movs.filter(m=>m.tipo==="cargo").reduce((a,m)=>a+m.monto,0))}</span>
            <span style={{ padding: "2px 8px", borderRadius: 8, background: "#fff", fontSize: 11, fontWeight: 600, color: "#2E7D32" }}>Abonos: {money(Math.abs(nota.movs.filter(m=>m.tipo==="abono").reduce((a,m)=>a+m.monto,0)))}</span>
          </div>
          {nota.movs.map((m, i) => (
            <div key={i} style={{ display: "grid", gridTemplateColumns: "90px 1fr 120px 110px", alignItems: "center", gap: 10, padding: "5px 0", borderBottom: i < nota.movs.length-1 ? ".5px solid #d8d8d8" : "none", fontSize: 12 }}>
              <span style={{ padding: "2px 8px", borderRadius: 6, fontSize: 10, fontWeight: 700, textAlign: "center", textTransform: "uppercase", letterSpacing: ".04em", background: m.tipo==="cargo" ? "rgba(33,150,243,.18)" : "rgba(76,175,80,.18)", color: m.tipo==="cargo" ? "#1565C0" : "#2E7D32" }}>{m.tipo}</span>
              <div><div>{m.concepto}</div><div style={{ fontSize: 10, color: "var(--gray-muted)" }}>{m.grupo}</div></div>
              <span style={{ fontSize: 11, color: "var(--gray-muted)" }}>{m.ref}</span>
              <span style={{ fontWeight: 700, textAlign: "right", fontVariantNumeric: "tabular-nums", color: m.tipo==="cargo" ? "#1565C0" : "#2E7D32" }}>{money(m.monto)}</span>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

function ReprocesoView({ isLoading, onRefresh }) {
  const [notas, setNotas] = React.useState(() => NOTAS_SEED.map(n => ({
    ...n,
    selected: !n.facturada && n.pago !== "efec",
    expanded: false,
  })));
  const [orden, setOrden] = React.useState({ campo: "num", asc: true });

  const toggleSelect = (i) => setNotas(notas.map((n, j) => j === i ? { ...n, selected: !n.selected } : n));
  const toggleExpand = (i) => setNotas(notas.map((n, j) => j === i ? { ...n, expanded: !n.expanded } : n));
  const selectAll = () => setNotas(notas.map(n => ({ ...n, selected: !n.facturada })));
  const deselectAll = () => setNotas(notas.map(n => ({ ...n, selected: false })));
  const setSort = (campo) => setOrden(o => o.campo === campo ? { campo, asc: !o.asc } : { campo, asc: true });

  const kpis = React.useMemo(() => {
    const total = notas.length;
    const sel   = notas.filter(n => n.selected && !n.facturada).length;
    const fact  = notas.filter(n => n.facturada).length;
    const unf   = total - fact;
    const tot   = notas.reduce((a, n) => a + n.total, 0);
    return { total, sel, fact, unf, tot };
  }, [notas]);

  const footers = React.useMemo(() => {
    const a = notas.filter(n => n.selected && !n.facturada).reduce((a, n) => a + n.total, 0);
    const b = notas.filter(n => !n.selected && !n.facturada).reduce((a, n) => a + n.total, 0);
    const c = notas.filter(n => n.facturada).reduce((a, n) => a + n.total, 0);
    return { a, b, c };
  }, [notas]);

  return (
    <React.Fragment>
      <ContentHeader
        title="REPROCESO"
        sub="clasificación de notas fiscales / no fiscales"
        right={<button className="btn-refresh" onClick={onRefresh}><MIcon name="refresh" />Actualizar</button>}
      />
      <div className="main-scroll" style={{ padding: 12, display: "flex", flexDirection: "column", gap: 8 }}>
        {/* Search bar */}
        <div style={{ background: "#fff", borderRadius: 10, boxShadow: "var(--shadow-2)", padding: "12px 16px", display: "flex", alignItems: "center", gap: 12, flexWrap: "wrap" }}>
          <MIcon name="calendar_today" size={22} color="var(--primary)" />
          <div style={{ position: "relative" }}>
            <span style={{ position: "absolute", top: -8, left: 12, background: "#fff", padding: "0 6px", fontSize: 10, color: "var(--primary)", fontWeight: 500 }}>Fecha inicio</span>
            <input style={{ border: "1.5px solid var(--primary)", borderRadius: 4, padding: "8px 12px", fontFamily: "var(--font)", fontSize: 13, minWidth: 160 }} defaultValue="21/05/2026" />
          </div>
          <span style={{ color: "var(--gray-muted)" }}>—</span>
          <div style={{ position: "relative" }}>
            <span style={{ position: "absolute", top: -8, left: 12, background: "#fff", padding: "0 6px", fontSize: 10, color: "var(--primary)", fontWeight: 500 }}>Fecha fin</span>
            <input style={{ border: "1.5px solid var(--primary)", borderRadius: 4, padding: "8px 12px", fontFamily: "var(--font)", fontSize: 13, minWidth: 160 }} defaultValue="21/05/2026" />
          </div>
          <div style={{ width: 1, background: "var(--gray-line)", height: 36 }}></div>
          <span style={{ flex: 1, fontSize: 12, fontStyle: "italic", color: "var(--gray-muted)" }}>{kpis.total} notas cargadas · {kpis.sel} seleccionadas para reproceso</span>
          <button className="btn btn-outlined" onClick={selectAll}><MIcon name="done_all" />Todas</button>
          <button className="btn btn-outlined" onClick={deselectAll}>Ninguna</button>
          <button className="btn btn-raised" disabled={isLoading}><MIcon name="search" />Buscar</button>
        </div>

        {/* Sort bar */}
        <div style={{ background: "#fff", borderRadius: 8, boxShadow: "var(--shadow)", padding: "7px 12px", display: "flex", alignItems: "center", gap: 4 }}>
          <span style={{ fontSize: 11, color: "var(--gray-muted)", display: "flex", alignItems: "center", gap: 6, marginRight: 8 }}>
            <MIcon name="sort" size={16} />Ordenar por:
          </span>
          {[{k:"num",l:"# Nota"},{k:"cliente",l:"Cliente"},{k:"pago",l:"Forma de pago"},{k:"total",l:"Total"},{k:"facturada",l:"Facturadas"},{k:"origen",l:"Origen"}].map(s => (
            <button key={s.k} className={"btn btn-flat" + (orden.campo === s.k ? " active" : "")} onClick={() => setSort(s.k)}>
              {s.l}
              <MIcon name={orden.campo === s.k ? (orden.asc ? "north" : "south") : "unfold_more"} size={14} color={orden.campo === s.k ? "var(--primary)" : "var(--gray-muted)"} />
            </button>
          ))}
        </div>

        {/* KPI row */}
        <div style={{ display: "grid", gridTemplateColumns: "repeat(5, 1fr)", gap: 8 }}>
          {[
            { lbl: "TOTAL NOTAS",   val: kpis.total, color: "var(--primary)", icon: "receipt" },
            { lbl: "SELECCIONADAS", val: kpis.sel,   color: "#9C27B0",        icon: "check_box" },
            { lbl: "FACTURADAS",    val: kpis.fact,  color: "#4CAF50",        icon: "check_circle" },
            { lbl: "SIN FACTURA",   val: kpis.unf,   color: "#FF9800",        icon: "error_outline" },
            { lbl: "TOTAL GENERAL", val: money(kpis.tot), color: "#323232",   icon: "payments", isMoney: true },
          ].map((k, i) => (
            <div key={i} style={{ background: "#fff", borderRadius: 8, boxShadow: "var(--shadow)", padding: 14 }}>
              <div style={{ display: "flex", alignItems: "center", gap: 6, marginBottom: 4 }}>
                <MIcon name={k.icon} size={16} color={k.color} />
                <span style={{ fontSize: 9, fontWeight: 700, color: "var(--gray-muted)", letterSpacing: ".08em" }}>{k.lbl}</span>
              </div>
              <div style={{ fontSize: k.isMoney ? 22 : 28, fontWeight: 700, lineHeight: 1, marginTop: 4, fontFamily: "var(--font-display)", color: k.color, fontVariantNumeric: "tabular-nums" }}>{k.val}</div>
            </div>
          ))}
        </div>

        {/* Notas list */}
        <div style={{ marginTop: 4 }}>
          {notas.map((n, i) => (
            <NotaRow
              key={n.num}
              nota={n}
              selected={n.selected}
              expanded={n.expanded}
              onSelect={() => toggleSelect(i)}
              onExpand={() => toggleExpand(i)}
            />
          ))}
        </div>
      </div>

      {/* Footer mini-KPIs + reprocesar */}
      <div style={{ background: "#fff", borderTop: "1px solid var(--gray-line)", padding: "10px 16px", display: "flex", alignItems: "center", gap: 12 }}>
        <div style={{ flex: 1, display: "flex", gap: 8 }}>
          {[
            { lbl: "A REPROCESAR",      val: footers.a, color: "#2E7D32", bg: "#E8F5E9", border: "#A5D6A7", icon: "check_circle" },
            { lbl: "VENTAS AL PÚBLICO", val: footers.b, color: "#BF360C", bg: "#FFF3E0", border: "#FFCC80", icon: "storefront" },
            { lbl: "YA FACTURADAS",     val: footers.c, color: "#0D47A1", bg: "#E3F2FD", border: "#90CAF9", icon: "receipt_long" },
          ].map((k, i) => (
            <div key={i} style={{ display: "flex", alignItems: "center", gap: 10, borderRadius: 8, padding: "10px 14px", background: k.bg, border: "1px solid " + k.border }}>
              <MIcon name={k.icon} size={20} color={k.color} />
              <div>
                <div style={{ fontSize: 9, fontWeight: 700, letterSpacing: ".08em", color: "var(--gray-muted)" }}>{k.lbl}</div>
                <div style={{ fontSize: 16, fontWeight: 700, color: k.color, fontVariantNumeric: "tabular-nums", marginTop: 1 }}>{money(k.val)}</div>
              </div>
            </div>
          ))}
        </div>
        <button className="btn btn-danger" disabled={isLoading || kpis.sel === 0}>
          <MIcon name="refresh" />
          <span style={{ display: "flex", flexDirection: "column", lineHeight: 1.1, alignItems: "flex-start", marginLeft: 4 }}>
            <span>Reprocesar notas</span>
            <span style={{ fontSize: 9, fontWeight: 500, opacity: .9, textTransform: "none", letterSpacing: 0 }}>{kpis.sel} seleccionada(s)</span>
          </span>
        </button>
      </div>
    </React.Fragment>
  );
}

Object.assign(window, { ReprocesoView });
