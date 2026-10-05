import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";
import { vi } from "vitest";
import CreateListing from "./CreateListing";
import { createListing } from "../../services/propertyListingService";

vi.mock("../../../../shared/components/owner/OwnerSidebar", () => ({
  default: () => <div>Owner Sidebar</div>,
}));

vi.mock("../../services/propertyListingService", () => ({
  createListing: vi.fn(),
}));

describe("CreateListing", () => {
  test("renders required property listing fields with validation constraints", () => {
    render(
      <MemoryRouter>
        <CreateListing />
      </MemoryRouter>
    );

    const title = screen.getByLabelText(/listing title/i);
    const description = screen.getByLabelText(/description/i);
    const price = screen.getByLabelText(/price/i);
    const address = screen.getByLabelText(/property address/i);
    const city = screen.getByLabelText(/city/i);

    expect(title).toBeRequired();
    expect(title).toHaveAttribute("minlength", "5");
    expect(title).toHaveAttribute("maxlength", "150");

    expect(description).toBeRequired();
    expect(description).toHaveAttribute("minlength", "20");
    expect(description).toHaveAttribute("maxlength", "2000");

    expect(price).toBeRequired();
    expect(price).toHaveAttribute("min", "1");

    expect(address).toBeRequired();
    expect(city).toBeRequired();
  });

  test("applies valid coordinate ranges", () => {
    render(
      <MemoryRouter>
        <CreateListing />
      </MemoryRouter>
    );

    const latitude = screen.getByLabelText(/latitude/i);
    const longitude = screen.getByLabelText(/longitude/i);

    expect(latitude).toHaveAttribute("min", "-90");
    expect(latitude).toHaveAttribute("max", "90");

    expect(longitude).toHaveAttribute("min", "-180");
    expect(longitude).toHaveAttribute("max", "180");
  });

    test("displays API error when listing creation fails", async () => {
    const user = userEvent.setup();

    createListing.mockRejectedValueOnce(
        new Error("Unable to create property listing.")
    );

    render(
        <MemoryRouter>
        <CreateListing />
        </MemoryRouter>
    );

    await user.type(
        screen.getByLabelText(/listing title/i),
        "Modern Colombo Apartment"
    );

    await user.type(
        screen.getByLabelText(/description/i),
        "A modern apartment with spacious rooms and convenient city access."
    );

    await user.type(
        screen.getByLabelText(/price/i),
        "25000000"
    );

    await user.type(
        screen.getByLabelText(/property address/i),
        "123 Galle Road"
    );

    await user.type(
        screen.getByLabelText(/city/i),
        "Colombo"
    );

    await user.click(
        screen.getByRole("button", { name: /create draft/i })
    );

    expect(
        await screen.findByText("Unable to create property listing.")
    ).toBeInTheDocument();

    expect(createListing).toHaveBeenCalledTimes(1);
    });
});