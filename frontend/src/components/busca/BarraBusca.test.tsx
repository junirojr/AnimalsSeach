import { render, screen, fireEvent, act } from "@testing-library/react";
import BarraBusca from "@/components/busca/BarraBusca";

describe("BarraBusca", () => {
  it("dispara aoBuscar com o termo digitado apos o debounce", () => {
    jest.useFakeTimers();
    const aoBuscar = jest.fn();

    render(<BarraBusca aoBuscar={aoBuscar} />);
    const input = screen.getByLabelText("Buscar animais");

    fireEvent.change(input, { target: { value: "gato" } });
    expect(aoBuscar).not.toHaveBeenCalledWith("gato");

    act(() => {
      jest.advanceTimersByTime(400);
    });
    expect(aoBuscar).toHaveBeenCalledWith("gato");

    jest.useRealTimers();
  });
});
