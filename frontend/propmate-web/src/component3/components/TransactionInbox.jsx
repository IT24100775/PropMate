import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { c3Api } from "../services/api";

export default function TransactionInbox({
    type,
    admin = false,
    embedded = false,
}) {
    const rental = type === "rental";

    const [items, setItems] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    useEffect(() => {
        let live = true;

        const loadTransactions = async () => {
            try {
                const data = admin
                    ? []
                    : rental
                        ? await c3Api.rentalOwner()
                        : await c3Api.purchaseOwner();

                if (live) {
                    setItems(data);
                }
            } catch (err) {
                if (live) {
                    setError(err.message);
                }
            } finally {
                if (live) {
                    setLoading(false);
                }
            }
        };

        loadTransactions();

        return () => {
            live = false;
        };
    }, [type, admin, rental]);

    if (admin) {
        return (
            <div className="c3-page">
                <h1>Component 3 — Admin view</h1>

                <p>
                    Admin access is view-only. Enter a transaction ID to inspect it.
                </p>

                <div className="c3-card">
                    <AdminLookup type={type} />
                </div>
            </div>
        );
    }

    const handleAccept = async (id) => {
        try {
            if (rental) {
                await c3Api.rentalAccept(id);
            } else {
                await c3Api.purchaseAccept(id);
            }

            setItems((currentItems) =>
                currentItems.map((item) =>
                    item.id === id ? { ...item, status: "Accepted" } : item
                )
            );
        } catch (err) {
            setError(err.message);
        }
    };

    const handleReject = async (id) => {
        try {
            if (rental) {
                await c3Api.rentalReject(id);
            } else {
                await c3Api.purchaseReject(id);
            }

            setItems((currentItems) =>
                currentItems.map((item) =>
                    item.id === id ? { ...item, status: "Rejected" } : item
                )
            );
        } catch (err) {
            setError(err.message);
        }
    };

    return (
        <div className={embedded ? "c3-embedded" : "c3-page"}>
            {!embedded && (
                <div className="c3-header">
                    <div>
                        <span>COMPONENT 3</span>

                        <h1>
                            {rental ? "Rental applications" : "Purchase offers"}
                        </h1>

                        <p>
                            Review submissions and open negotiations for your listings.
                        </p>
                    </div>
                </div>
            )}

            {error && <div className="dashboard-error">{error}</div>}

            {loading ? (
                <div className="c3-owner-empty">
                    <p>Loading transactions...</p>
                </div>
            ) : items.length === 0 ? (
                <div className="c3-owner-empty">
                    <span className="c3-empty-number">01</span>

                    <h3>
                        {rental
                            ? "No rental applications yet."
                            : "No purchase offers yet."}
                    </h3>

                    <p>
                        {rental
                            ? "Rental applications submitted for your properties will appear here."
                            : "Purchase offers submitted for your properties will appear here."}
                    </p>
                </div>
            ) : (
                <div className="c3-owner-list">
                    {items.map((item) => (
                        <article className="c3-owner-card" key={item.id}>
                            <div className="c3-owner-card-top">
                                <div>
                                    <p className="c3-property-label">
                                        {item.propertyTitle}
                                    </p>

                                    <h2>
                                        #{item.id} ·{" "}
                                        {rental ? item.tenantName : item.buyerName}
                                    </h2>
                                </div>

                                <span
                                    className={`c3-status c3-status-${String(
                                        item.status
                                    ).toLowerCase()}`}
                                >
                                    {item.status}
                                </span>
                            </div>

                            <div className="c3-owner-details">
                                {rental ? (
                                    <>
                                        <div>
                                            <span>MONTHLY INCOME</span>
                                            <strong>
                                                LKR{" "}
                                                {Number(item.monthlyIncome || 0).toLocaleString(
                                                    "en-LK"
                                                )}
                                            </strong>
                                        </div>

                                        <div>
                                            <span>OCCUPANTS</span>
                                            <strong>{item.occupants}</strong>
                                        </div>

                                        <div>
                                            <span>PREFERRED MOVE-IN</span>
                                            <strong>
                                                {item.preferredMoveInDate || "Not specified"}
                                            </strong>
                                        </div>
                                    </>
                                ) : (
                                    <div>
                                        <span>OFFER AMOUNT</span>
                                        <strong>
                                            LKR{" "}
                                            {Number(item.offerAmount || 0).toLocaleString(
                                                "en-LK"
                                            )}
                                        </strong>
                                    </div>
                                )}
                            </div>

                            <p className="c3-owner-message">
                                {item.message ||
                                    item.conditions ||
                                    "No additional message."}
                            </p>

                            <div className="c3-owner-actions">
                                <Link
                                    className="c3-open-button"
                                    to={`/owner/component3/${type}/${item.id}`}
                                >
                                    Open negotiation <span>→</span>
                                </Link>

                                {item.status === "Pending" && (
                                    <>
                                        <button
                                            type="button"
                                            className="c3-accept-button"
                                            onClick={() => handleAccept(item.id)}
                                        >
                                            Accept
                                        </button>

                                        <button
                                            type="button"
                                            className="c3-reject-button"
                                            onClick={() => handleReject(item.id)}
                                        >
                                            Reject
                                        </button>
                                    </>
                                )}
                            </div>
                        </article>
                    ))}
                </div>
            )}
        </div>
    );
}

function AdminLookup({ type }) {
    const [id, setId] = useState("");
    const [data, setData] = useState(null);
    const [error, setError] = useState("");

    const handleView = async () => {
        try {
            const result =
                type === "rental"
                    ? await c3Api.rentalGet(id)
                    : await c3Api.purchaseGet(id);

            setData(result);
            setError("");
        } catch (err) {
            setError(err.message);
        }
    };

    return (
        <>
            <input
                placeholder="Transaction ID"
                value={id}
                onChange={(event) => setId(event.target.value)}
            />

            <button type="button" onClick={handleView}>
                View
            </button>

            {error && <div className="c3-error">{error}</div>}

            {data && <pre>{JSON.stringify(data, null, 2)}</pre>}
        </>
    );
}