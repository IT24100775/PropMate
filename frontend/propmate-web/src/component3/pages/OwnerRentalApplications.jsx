import OwnerSidebar from "../../components/owner/OwnerSidebar";
import TransactionInbox from "../components/TransactionInbox";
import "../../pages/owner/OwnerDashboard.css";
import "../styles/component3.css";

export default function OwnerRentalApplications() {
    return (
        <div className="owner-layout">
            <OwnerSidebar />

            <main className="owner-main">
                <header className="owner-header">
                    <div>
                        <p className="page-eyebrow">TRANSACTIONS</p>
                        <h1>Rental applications</h1>
                        <p className="page-description">
                            Review rental applications and manage negotiations for your
                            published properties.
                        </p>
                    </div>
                </header>

                <TransactionInbox type="rental" embedded />
            </main>
        </div>
    );
}