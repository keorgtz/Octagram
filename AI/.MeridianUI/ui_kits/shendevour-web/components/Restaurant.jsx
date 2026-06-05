/* eslint-disable */
// ─────────────────────────────────────────────────────────────
// Restaurant view — rack de mesas + acciones rápidas + dock de pedidos
// ─────────────────────────────────────────────────────────────

const R_STATES = {
  available: { label: "Libre",       border: "#D1D5DB", bg: "#FFFFFF", fg: "#6B7280", chairBg: "#E5E7EB" },
  occupied:  { label: "Ocupada",     border: "#6366F1", bg: "#EEF2FF", fg: "#4338CA", chairBg: "#A5B4FC" },
  reserved:  { label: "Reservada",   border: "#F59E0B", bg: "#FEF3C7", fg: "#92400E", chairBg: "#FBBF24" },
  cleaning:  { label: "Limpiando",   border: "#8B5CF6", bg: "#EDE9FE", fg: "#6D28D9", chairBg: "#C4B5FD" },
  attention: { label: "Atención",    border: "#F97316", bg: "#FFEDD5", fg: "#9A3412", chairBg: "#FB923C" },
};

const R_TABLES = [
  // Salón principal — frente
  { id: "01", x:  20, y:  16, w: 88, h: 88, shape: "round", seats: 4, state: "occupied",  guests: 3, since: 42, server: "María" },
  { id: "02", x: 140, y:  16, w: 88, h: 88, shape: "round", seats: 4, state: "available" },
  { id: "03", x: 260, y:  16, w: 88, h: 88, shape: "round", seats: 4, state: "reserved",  resBy: "Soto",  resAt: "14:30" },
  { id: "04", x: 380, y:  16, w: 88, h: 88, shape: "round", seats: 2, state: "occupied",  guests: 2, since: 18, server: "Luis" },
  { id: "05", x: 500, y:  16, w: 88, h: 88, shape: "round", seats: 4, state: "available" },
  // Salón principal — mesas largas
  { id: "06", x:  20, y: 136, w: 200, h: 76, shape: "rect", seats: 6, state: "attention", guests: 6, since: 85, server: "Pedro" },
  { id: "07", x: 250, y: 136, w: 200, h: 76, shape: "rect", seats: 6, state: "occupied",  guests: 4, since: 30, server: "María" },
  { id: "08", x: 480, y: 136, w: 108, h: 76, shape: "rect", seats: 4, state: "cleaning" },
  // Terraza
  { id: "T1", x:  20, y: 264, w: 88, h: 88, shape: "round", seats: 4, state: "occupied",  guests: 4, since: 12, server: "Luis" },
  { id: "T2", x: 140, y: 264, w: 88, h: 88, shape: "round", seats: 4, state: "available" },
  { id: "T3", x: 260, y: 264, w: 88, h: 88, shape: "round", seats: 4, state: "reserved",  resBy: "Vega",  resAt: "15:00" },
  { id: "T4", x: 380, y: 264, w: 88, h: 88, shape: "round", seats: 4, state: "available" },
  { id: "T5", x: 500, y: 264, w: 88, h: 88, shape: "round", seats: 4, state: "occupied",  guests: 2, since: 8, server: "Ana" },
];

const R_ZONES = [
  { label: "Salón principal", y: 0,   h: 220 },
  { label: "Terraza",         y: 240, h: 130 },
];

// Chair positions around a table (relative offsets from table edges)
function restChairs(table) {
  const cs = [];
  if (table.shape === "round") {
    // 4 chairs at N, S, E, W
    cs.push({ side: "top",    left: "50%", top: "-8px",  tx: -8, ty: 0 });
    cs.push({ side: "bottom", left: "50%", top: "calc(100% - 8px)", tx: -8, ty: 0 });
    cs.push({ side: "left",   left: "-8px", top: "50%", tx: 0, ty: -8 });
    cs.push({ side: "right",  left: "calc(100% - 8px)", top: "50%", tx: 0, ty: -8 });
  } else {
    // 3 chairs top, 3 bottom (for 6-seat) or 2+2 for smaller
    const n = Math.max(2, Math.min(4, Math.floor(table.seats / 2)));
    for (let i = 0; i < n; i++) {
      const left = `${((i + 1) / (n + 1)) * 100}%`;
      cs.push({ side: "top",    left, top: "-8px",  tx: -8, ty: 0 });
      cs.push({ side: "bottom", left, top: "calc(100% - 8px)", tx: -8, ty: 0 });
    }
  }
  return cs;
}

function RestFloorTable({ table, selected, onSelect }) {
  const s = R_STATES[table.state];
  const isRound = table.shape === "round";
  return (
    <div
      onClick={() => onSelect(table.id)}
      style={{
        position: "absolute",
        left: table.x, top: table.y,
        width: table.w, height: table.h,
        cursor: "pointer",
        transition: "transform 120ms ease",
      }}
    >
      {/* chairs */}
      {restChairs(table).map((c, i) => (
        <div key={i} style={{
          position: "absolute",
          left: c.left, top: c.top,
          width: 16, height: 16,
          borderRadius: "50%",
          background: s.chairBg,
          transform: `translate(${c.tx}px, ${c.ty}px)`,
          zIndex: 0,
          transition: "background 120ms ease",
        }}></div>
      ))}
      {/* table surface */}
      <div style={{
        position: "absolute", inset: 0,
        background: s.bg,
        border: `2px solid ${s.border}`,
        borderRadius: isRound ? "50%" : 12,
        display: "flex", flexDirection: "column",
        alignItems: "center", justifyContent: "center",
        boxShadow: selected ? "0 0 0 3px rgba(99,102,241,.25), var(--shadow)" : "var(--shadow)",
        zIndex: 1,
        gap: 2,
      }}>
        <div style={{
          fontSize: isRound ? 22 : 20,
          fontWeight: 800, color: s.fg,
          lineHeight: 1,
          fontVariantNumeric: "tabular-nums",
        }}>{table.id}</div>
        {table.state === "occupied" && (
          <div style={{ fontSize: 10, fontWeight: 600, color: s.fg, opacity: .85, display: "flex", alignItems: "center", gap: 3 }}>
            <MIcon name="group" size={11} color={s.fg} />{table.guests}
            <span style={{ opacity: .5, margin: "0 1px" }}>·</span>
            <MIcon name="schedule" size={11} color={s.fg} />{table.since}m
          </div>
        )}
        {table.state === "reserved" && (
          <div style={{ fontSize: 10, fontWeight: 600, color: s.fg, textAlign: "center", lineHeight: 1.2 }}>
            {table.resBy}<br/>{table.resAt}
          </div>
        )}
        {table.state === "attention" && (
          <div style={{ fontSize: 10, fontWeight: 700, color: s.fg, textTransform: "uppercase", letterSpacing: ".04em" }}>
            <MIcon name="priority_high" size={12} color={s.fg} fill />
          </div>
        )}
        {table.state === "cleaning" && (
          <MIcon name="mop" size={16} color={s.fg} />
        )}
      </div>
    </div>
  );
}

// Quick-order cards (dock)
const R_ORDERS = [
  { mesa: "01", client: "Garcia, L.",   party: 3, items: 5, total:  640, since: 12, status: "kitchen",  server: "María" },
  { mesa: "04", client: "Pareja",        party: 2, items: 3, total:  420, since:  8, status: "served",   server: "Luis"  },
  { mesa: "06", client: "Familia Pérez", party: 6, items: 9, total: 1850, since: 25, status: "bill",     server: "Pedro" },
  { mesa: "07", client: "Hernández",     party: 4, items: 6, total:  980, since: 18, status: "kitchen",  server: "María" },
  { mesa: "T1", client: "Walk-in",       party: 4, items: 4, total:  560, since:  6, status: "draft",    server: "Luis"  },
  { mesa: "T5", client: "Reyes, P.",     party: 2, items: 2, total:  280, since:  4, status: "draft",    server: "Ana"   },
  { mesa: "BAR", client: "Barra · 3",    party: 3, items: 5, total:  340, since: 14, status: "served",   server: "Ana"   },
  { mesa: "TO-GO", client: "Mtz. (recoge)", party: 1, items: 4, total:  480, since: 22, status: "ready",  server: "—"   },
];

const R_ORDER_STATUS = {
  draft:   { label: "Borrador",      bg: "#F3F4F6", fg: "#374151", icon: "edit_note" },
  kitchen: { label: "En cocina",     bg: "#FEF3C7", fg: "#92400E", icon: "soup_kitchen" },
  ready:   { label: "Listo",         bg: "#DCFCE7", fg: "#065F46", icon: "room_service" },
  served:  { label: "Servido",       bg: "#E0E7FF", fg: "#4338CA", icon: "restaurant" },
  bill:    { label: "Cuenta",        bg: "#EDE9FE", fg: "#6D28D9", icon: "receipt_long" },
};

function RestOrderCard({ order, active, onClick }) {
  const st = R_ORDER_STATUS[order.status];
  return (
    <div onClick={onClick} style={{
      flex: "0 0 240px",
      background: "#fff",
      borderRadius: 12,
      padding: "10px 12px",
      boxShadow: active ? "0 0 0 2px var(--in), var(--shadow)" : "var(--shadow)",
      cursor: "pointer",
      display: "flex", flexDirection: "column", gap: 6,
      transition: "box-shadow 120ms ease",
    }}>
      <div style={{ display: "flex", alignItems: "center", gap: 8 }}>
        <span style={{
          background: "#1C1E26",
          color: "#fff",
          padding: "3px 8px",
          borderRadius: 6,
          fontSize: 11,
          fontWeight: 800,
          letterSpacing: ".04em",
          fontVariantNumeric: "tabular-nums",
        }}>{order.mesa}</span>
        <span style={{ fontSize: 13, fontWeight: 600, color: "var(--gray-dark)", overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap", flex: 1 }}>{order.client}</span>
        <span style={{ fontSize: 10, color: "var(--gray-muted)", display: "flex", alignItems: "center", gap: 2 }}>
          <MIcon name="group" size={11} />{order.party}
        </span>
      </div>
      <div style={{ display: "flex", alignItems: "center", gap: 8, fontSize: 11.5, color: "var(--gray-muted)" }}>
        <span style={{ display: "flex", alignItems: "center", gap: 3 }}>
          <MIcon name="restaurant_menu" size={13} />{order.items} ítems
        </span>
        <span style={{ display: "flex", alignItems: "center", gap: 3 }}>
          <MIcon name="schedule" size={13} />{order.since}m
        </span>
        <span style={{ marginLeft: "auto", color: "var(--gray-dark)", fontWeight: 700, fontVariantNumeric: "tabular-nums" }}>{money(order.total)}</span>
      </div>
      <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between" }}>
        <span style={{
          display: "inline-flex", alignItems: "center", gap: 4,
          background: st.bg, color: st.fg,
          padding: "3px 8px", borderRadius: 999,
          fontSize: 10.5, fontWeight: 700,
        }}>
          <MIcon name={st.icon} size={13} color={st.fg} />{st.label}
        </span>
        <span style={{ fontSize: 10.5, color: "var(--gray-muted)" }}>{order.server}</span>
      </div>
    </div>
  );
}

// Mini-KPI strip card
function RestMiniMetric({ icon, label, value, subValue, color, bg }) {
  return (
    <div style={{
      background: "#fff",
      borderRadius: 14,
      padding: "12px 16px",
      boxShadow: "var(--shadow)",
      display: "flex", alignItems: "center", gap: 12,
      minWidth: 0, flex: 1,
    }}>
      <div style={{
        width: 38, height: 38,
        borderRadius: 10,
        background: bg, display: "flex", alignItems: "center", justifyContent: "center",
        flexShrink: 0,
      }}>
        <MIcon name={icon} size={20} color={color} fill />
      </div>
      <div style={{ minWidth: 0, flex: 1 }}>
        <div style={{ fontSize: 10, fontWeight: 700, color: "var(--gray-muted)", textTransform: "uppercase", letterSpacing: ".08em" }}>{label}</div>
        <div style={{ display: "flex", alignItems: "baseline", gap: 6, marginTop: 2 }}>
          <span style={{ fontSize: 22, fontWeight: 700, color: color, lineHeight: 1, fontVariantNumeric: "tabular-nums" }}>{value}</span>
          {subValue && <span style={{ fontSize: 11, color: "var(--gray-muted)" }}>{subValue}</span>}
        </div>
      </div>
    </div>
  );
}

function RestaurantView({ isLoading, onRefresh }) {
  const [selected, setSelected] = React.useState(null);
  const [activeOrder, setActiveOrder] = React.useState(0);
  const [view, setView] = React.useState("mesa"); // mesa | lista

  const occCount = R_TABLES.filter(t => t.state === "occupied" || t.state === "attention").length;
  const totalTables = R_TABLES.length;
  const activeOrders = R_ORDERS.length;
  const dayRevenue = R_ORDERS.reduce((a, o) => a + o.total, 0) + 12480;
  const avgTicket = Math.round(dayRevenue / (occCount + 4));

  return (
    <React.Fragment>
      <ContentHeader
        title="Restaurante"
        sub="turno matutino · 12 mesas · 2 zonas"
        right={
          <React.Fragment>
            {isLoading && <span style={{ width: 22, height: 22, border: "3px solid rgba(99,102,241,.2)", borderTopColor: "#6366F1", borderRadius: "50%", animation: "spin .8s linear infinite", display: "inline-block" }}></span>}
            <button className="btn-refresh" onClick={onRefresh}><MIcon name="refresh" />Actualizar</button>
          </React.Fragment>
        }
      />

      <div className="main-scroll" style={{ padding: "12px 14px 8px", display: "flex", flexDirection: "column", gap: 10 }}>

        {/* METRICS STRIP */}
        <div style={{ display: "grid", gridTemplateColumns: "repeat(4, 1fr)", gap: 10 }}>
          <RestMiniMetric icon="point_of_sale"     label="Ventas hoy"     value={money(dayRevenue)} subValue={`avg ${money(avgTicket)}`} color="#10B981" bg="#DCFCE7" />
          <RestMiniMetric icon="table_restaurant"  label="Mesas ocupadas" value={`${occCount} / ${totalTables}`} subValue={`${Math.round(occCount/totalTables*100)}%`} color="#6366F1" bg="#E0E7FF" />
          <RestMiniMetric icon="receipt"           label="Pedidos activos" value={activeOrders} subValue="3 en cocina" color="#F59E0B" bg="#FEF3C7" />
          <RestMiniMetric icon="trending_up"       label="Ticket promedio" value={money(avgTicket)} subValue="+8% vs ayer" color="#8B5CF6" bg="#EDE9FE" />
        </div>

        {/* ACTION BAR */}
        <div style={{
          background: "#fff", borderRadius: 14, padding: "10px 14px",
          boxShadow: "var(--shadow)",
          display: "flex", alignItems: "center", gap: 10,
        }}>
          {/* Search */}
          <div style={{ position: "relative", flex: "0 0 280px" }}>
            <MIcon name="search" size={18} color="var(--gray-muted)" style={{ position: "absolute", left: 10, top: "50%", transform: "translateY(-50%)" }} />
            <input
              placeholder="Buscar mesa, mesero, cliente o platillo…"
              style={{ width: "100%", border: "1.5px solid var(--gray-line)", borderRadius: 10, padding: "8px 12px 8px 36px", fontFamily: "var(--font)", fontSize: 12.5, outline: "none" }}
            />
          </div>

          {/* Quick actions */}
          <div style={{ display: "flex", alignItems: "center", gap: 6, flex: 1 }}>
            <button className="btn btn-raised"><MIcon name="add_circle" fill />Nuevo pedido</button>
            <button className="btn btn-outlined"><MIcon name="event_available" />Reservar mesa</button>
            <button className="btn btn-flat"><MIcon name="local_dining" />Combinar mesas</button>
            <button className="btn btn-flat"><MIcon name="tune" />Filtros</button>
          </div>

          {/* View toggle */}
          <div style={{ display: "flex", background: "var(--gray-foot)", borderRadius: 10, padding: 3, gap: 2 }}>
            {["mesa", "lista"].map(v => (
              <button
                key={v}
                onClick={() => setView(v)}
                style={{
                  border: "none", background: view === v ? "#fff" : "transparent",
                  boxShadow: view === v ? "var(--shadow)" : "none",
                  padding: "5px 10px", borderRadius: 7, cursor: "pointer",
                  display: "flex", alignItems: "center", gap: 5,
                  fontFamily: "var(--font)", fontSize: 12,
                  fontWeight: view === v ? 600 : 500,
                  color: view === v ? "var(--gray-dark)" : "var(--gray-muted)",
                }}
              >
                <MIcon name={v === "mesa" ? "grid_view" : "list_alt"} size={15} />
                {v === "mesa" ? "Mesas" : "Lista"}
              </button>
            ))}
          </div>
        </div>

        {/* FLOOR PLAN */}
        <div style={{
          background: "#fff", borderRadius: 14, padding: "12px 14px",
          boxShadow: "var(--shadow)",
          flex: 1, minHeight: 0,
          display: "flex", flexDirection: "column",
        }}>
          <div style={{ display: "flex", alignItems: "center", gap: 10, marginBottom: 6, flexShrink: 0, flexWrap: "wrap" }}>
            <span style={{ fontSize: 10, fontWeight: 700, color: "var(--gray-muted)", textTransform: "uppercase", letterSpacing: ".08em" }}>RACK DE MESAS</span>
            <span style={{ flex: 1 }}></span>
            {Object.entries(R_STATES).map(([k, v]) => (
              <div key={k} style={{ display: "flex", alignItems: "center", gap: 5, fontSize: 10.5, color: "var(--gray-muted)" }}>
                <span style={{ width: 10, height: 10, borderRadius: "50%", background: v.bg, border: `2px solid ${v.border}` }}></span>
                {v.label}
              </div>
            ))}
          </div>
          <div style={{ position: "relative", flex: 1, minHeight: 380, background: "linear-gradient(180deg, #FAFBFC 0%, #F4F6FA 100%)", borderRadius: 10, padding: 8, overflow: "hidden" }}>
            {R_ZONES.map(z => (
              <div key={z.label} style={{
                position: "absolute",
                left: 10, top: z.y + 8,
                fontSize: 9, fontWeight: 700,
                color: "var(--gray-muted)",
                textTransform: "uppercase", letterSpacing: ".1em",
                writingMode: "vertical-rl",
                transform: "rotate(180deg)",
                height: z.h,
                display: "flex", alignItems: "center",
              }}>{z.label}</div>
            ))}
            <div style={{ position: "absolute", left: 32, right: 16, top: 232, height: 1, borderTop: "1px dashed var(--gray-line)" }}></div>
            <div style={{ position: "absolute", left: 36, top: 8, right: 8, bottom: 8 }}>
              {R_TABLES.map(t => (
                <RestFloorTable key={t.id} table={t} selected={selected === t.id} onSelect={setSelected} />
              ))}
            </div>
          </div>
        </div>
      </div>

      {/* ORDERS DOCK — barra inferior fija */}
      <div style={{
        background: "#fff",
        borderTop: "1px solid var(--gray-line)",
        boxShadow: "0 -2px 8px rgba(0,0,0,.04)",
        padding: "8px 14px 10px",
        flexShrink: 0,
        display: "flex", flexDirection: "column", gap: 6,
      }}>
        <div style={{ display: "flex", alignItems: "center", gap: 8, padding: "0 4px" }}>
          <span style={{ fontSize: 10, fontWeight: 700, color: "var(--gray-muted)", textTransform: "uppercase", letterSpacing: ".08em" }}>Pedidos rápidos</span>
          <span style={{ background: "#1C1E26", color: "#fff", borderRadius: 999, padding: "2px 8px", fontSize: 10, fontWeight: 700 }}>{R_ORDERS.length}</span>
          <span style={{ flex: 1 }}></span>
          <button className="btn btn-flat" style={{ fontSize: 11, height: 26 }}><MIcon name="filter_list" size={14} />Por estado</button>
          <button className="btn btn-flat" style={{ fontSize: 11, height: 26 }}><MIcon name="visibility" size={14} />Ver todos</button>
        </div>
        <div style={{
          display: "flex", gap: 10, overflowX: "auto",
          paddingBottom: 4, paddingTop: 2, paddingLeft: 4, paddingRight: 4,
          scrollbarWidth: "thin",
        }}>
          {R_ORDERS.map((o, i) => (
            <RestOrderCard key={i} order={o} active={activeOrder === i} onClick={() => setActiveOrder(i)} />
          ))}
        </div>
      </div>
    </React.Fragment>
  );
}

Object.assign(window, { RestaurantView });
