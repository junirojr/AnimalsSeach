interface LogoAnimalsearchProps {
  size?: number;
  cor?: string;
}

export default function LogoAnimalsearch({ size = 96, cor = "#00e5cc" }: LogoAnimalsearchProps) {
  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 100 100"
      fill="none"
      stroke={cor}
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden
    >
      {/* Círculo externo tracejado */}
      <circle cx="50" cy="50" r="44" strokeWidth="3.8" strokeDasharray="20 14" />

      {/* Forma do olho (lens) */}
      <path d="M16 50 C25 27 75 27 84 50 C75 73 25 73 16 50Z" strokeWidth="3.8" />

      {/* Lente da lupa (sobreposta ao olho) */}
      <circle cx="48" cy="47" r="12.5" strokeWidth="3.8" />

      {/* Cabo da lupa */}
      <line x1="57" y1="56" x2="70" y2="69" strokeWidth="4.2" />

      {/* Cabeça da pessoa */}
      <circle cx="50" cy="73" r="6.5" strokeWidth="3.5" />

      {/* Ombros da pessoa */}
      <path d="M37 85 Q50 79 63 85" strokeWidth="3.5" />
    </svg>
  );
}
