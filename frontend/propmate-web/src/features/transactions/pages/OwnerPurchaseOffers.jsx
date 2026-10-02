import OwnerSidebar from "../../../shared/components/owner/OwnerSidebar";
import TransactionInbox from "../components/TransactionInbox";
import "../../../pages/owner/OwnerDashboard.css";
import "../styles/component3.css";

export default function OwnerPurchaseOffers() {
    return (
        <div className="owner-layout">
            <OwnerSidebar />

            <main className="owner-main">
                <header className="owner-header">
                    <div>
                        <p className="page-eyebrow">TRANSACTIONS</p>
                        <h1>Purchase offers</h1>
                        <p className="page-description">
                            Review purchase offers and manage negotiations for your
                            published properties.
                        </p>
                    </div>
                </header>

                <TransactionInbox type="purchase" embedded />
            </main>
        </div>
    );
}