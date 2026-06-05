/* eslint-disable */
// ─────────────────────────────────────────────────────────────
// MeridianUI atoms — Icon, Chip, Button helpers
// ─────────────────────────────────────────────────────────────

function MIcon({ name, size, color, className, style, fill }) {
  const s = { fontSize: size, color, ...style };
  if (fill) s.fontVariationSettings = "'FILL' 1, 'wght' 500, 'GRAD' 0, 'opsz' 24";
  return <span className={"mi material-symbols-rounded " + (className || "")} style={s}>{name}</span>;
}

function Chip({ kind, icon, children }) {
  return (
    <span className={"chip " + (kind || "")}>
      {icon && <MIcon name={icon} />}
      {children}
    </span>
  );
}

function Checkbox({ checked, disabled, onChange }) {
  return (
    <span
      className={"cb" + (checked ? " checked" : "") + (disabled ? " dis" : "")}
      onClick={() => !disabled && onChange && onChange(!checked)}
      role="checkbox"
      aria-checked={checked}
    />
  );
}

// Format helpers — Mexican peso, tabular nums
function money(n, opts = {}) {
  const sign = n < 0 ? "– " : "";
  const abs = Math.abs(n);
  return sign + "$ " + abs.toLocaleString("es-MX", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}
function moneyShort(n) {
  const sign = n < 0 ? "– " : "";
  const abs = Math.abs(n);
  return sign + "$ " + abs.toLocaleString("es-MX", { maximumFractionDigits: 0 });
}

Object.assign(window, { MIcon, Chip, Checkbox, money, moneyShort });
