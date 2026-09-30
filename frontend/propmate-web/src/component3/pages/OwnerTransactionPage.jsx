import { Link, useParams } from "react-router-dom";
import OwnerSidebar from "../../components/owner/OwnerSidebar";
import NegotiationPanel from "../components/NegotiationPanel";
import "../../pages/owner/OwnerDashboard.css";
import "../styles/component3.css";

export default function OwnerTransactionPage() {
    const { type, id } = useParams();

    const rental = type === "rental";

    return (
        <div className="owner-layout">
            <OwnerSidebar />

            <main className="owner-main">
                <header className="owner-header c3-transaction-header">
                    <div>
                        <p className="page-eyebrow">TRANSACTION NEGOTIATION</p>

                        <h1>
                            {rental
                                ? `Rental negotiation #${id}`
                                : `Purchase negotiation #${id}`}
                        </h1>

                        <p className="page-description">
                            Review counter-offers, communicate with the applicant,
                            and manage the final agreement.
                        </p>
                    </div>

                    <Link
                        className="c3-back-button"
                        to={
                            rental
                                ? "/owner/component3/rentals"
                                : "/owner/component3/purchases"
                        }
                    >
                        ← Back to {rental ? "applications" : "offers"}
                    </Link>
                </header>

                <NegotiationPanel
                    type={type}
                    id={Number(id)}
                    embedded
                />
            </main>
        </div>
    );
}