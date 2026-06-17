export default function NavegacaoTopo() {
  return (
    <header
      className="fixed inset-x-0 top-0 z-40 flex h-14 items-center justify-center px-4"
      style={{
        background: "rgba(9,23,22,0.92)",
        backdropFilter: "blur(12px)",
        borderBottom: "1px solid #1a3330",
      }}
    >
      <span className="font-semibold tracking-wide text-white">Animalsearch</span>
    </header>
  );
}