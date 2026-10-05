import { useEffect, useState } from "react";
import { c3Api } from "../services/api";
import AiAssistant from "./AiAssistant";

export default function NegotiationPanel({
    type,
    id,
    title,
    embedded = false,
}) {
    const rental = type === "rental";

    // Current logged-in user
    const savedUser = JSON.parse(
        localStorage.getItem("propmate_user") || "null"
    );

    const currentUserId = Number(savedUser?.id);
    const currentRole = savedUser?.role;

    // Transaction data
    const [transaction, setTransaction] = useState(null);
    const [offers, setOffers] = useState([]);
    const [messages, setMessages] = useState([]);
    const [agreement, setAgreement] = useState(null);
    const [submittingCounter, setSubmittingCounter] = useState(false);

    // Form data
    const [message, setMessage] = useState("");
    const [amount, setAmount] = useState("");
    const [rent, setRent] = useState("");
    const [moveIn, setMoveIn] = useState("");
    const [duration, setDuration] = useState("12");
    const [conditions, setConditions] = useState("");

    const [error, setError] = useState("");

    // =========================
    // LOAD TRANSACTION DATA
    // =========================

    const load = async () => {
        try {
            setError("");

            const [
                transactionData,
                offerData,
                messageData,
                agreementData,
            ] = await Promise.all([
                rental
                    ? c3Api.rentalGet(id)
                    : c3Api.purchaseGet(id),

                rental
                    ? c3Api.rentalOffers(id)
                    : c3Api.purchaseOffers(id),

                rental
                    ? c3Api.rentalMessages(id)
                    : c3Api.purchaseMessages(id),

                rental
                    ? c3Api.rentalAgreement(id)
                    : c3Api.purchaseAgreement(id),
            ]);

            setTransaction(transactionData);
            setOffers(offerData || []);
            setMessages(messageData || []);
            setAgreement(agreementData);
        } catch (err) {
            setError(err.message);
        }
    };

    useEffect(() => {
        load();
    }, [id, type]);

    // =========================
    // TRANSACTION STATE RULES
    // =========================

    const negotiationOpen =
        transaction?.negotiationStatus === "Open";

    const negotiationClosed =
        transaction?.negotiationStatus === "ClosedAccepted" ||
        transaction?.negotiationStatus === "ClosedRejected";

    const completed =
        transaction?.status === "Completed";

    // A counter-offer can start negotiation from Pending,
    // or continue while negotiation is already open.
    const canCounter =
        !completed &&
        !negotiationClosed &&
        (
            transaction?.status === "Pending" ||
            transaction?.status === "InNegotiation"
        );

    // Backend only allows messages while negotiation is Open.
    const canMessage =
        !completed && negotiationOpen;

    // Determine whether the currently logged-in party
    // has already confirmed the agreement.
    const alreadyConfirmed =
        currentRole === "OwnerAgent"
            ? agreement?.sellerConfirmed
            : currentRole === "BuyerRenter"
                ? agreement?.buyerConfirmed
                : true;

    const canConfirm =
        agreement &&
        !completed &&
        !alreadyConfirmed;

    // =========================
    // COUNTER OFFER
    // =========================

    const counter = async () => {
        // Prevent repeated clicks from creating duplicate counter-offers.
        if (submittingCounter) {
            return;
        }

        if (!canCounter) {
            setError(
                "Counter-offers are not available for this transaction."
            );
            return;
        }

        try {
            setSubmittingCounter(true);
            setError("");

            if (rental) {
                if (
                    !rent ||
                    !moveIn ||
                    !duration ||
                    Number(rent) <= 0 ||
                    Number(duration) <= 0
                ) {
                    setError(
                        "Enter a valid monthly rent, move-in date and duration."
                    );
                    return;
                }

                await c3Api.rentalCounter(id, {
                    monthlyRent: Number(rent),
                    moveInDate: moveIn,
                    durationMonths: Number(duration),
                    conditions,
                });
            } else {
                if (!amount || Number(amount) <= 0) {
                    setError(
                        "Enter a valid offer amount."
                    );
                    return;
                }

                await c3Api.purchaseCounter(id, {
                    offerAmount: Number(amount),
                    conditions,
                });
            }

            setRent("");
            setAmount("");
            setMoveIn("");
            setConditions("");

            await load();
        } catch (err) {
            setError(err.message);
        } finally {
            setSubmittingCounter(false);
        }
    };

    // =========================
    // MESSAGE
    // =========================

    const send = async () => {
        if (!message.trim()) {
            return;
        }

        if (!canMessage) {
            setError(
                "Messages can only be sent while negotiation is open."
            );
            return;
        }

        try {
            setError("");

            if (rental) {
                await c3Api.rentalMessage(id, {
                    message: message.trim(),
                });
            } else {
                await c3Api.purchaseMessage(id, {
                    message: message.trim(),
                });
            }

            setMessage("");

            await load();
        } catch (err) {
            setError(err.message);
        }
    };

    // =========================
    // ACCEPT COUNTER OFFER
    // =========================

    const accept = async (offer) => {
        // A user cannot accept their own proposal.
        if (Number(offer.proposedByUserId) === currentUserId) {
            setError(
                "You cannot accept your own counter-offer."
            );
            return;
        }

        if (!negotiationOpen) {
            setError(
                "Counter-offers can only be accepted while negotiation is open."
            );
            return;
        }

        try {
            setError("");

            if (rental) {
                await c3Api.rentalAcceptCounter(
                    id,
                    offer.id
                );
            } else {
                await c3Api.purchaseAcceptCounter(
                    id,
                    offer.id
                );
            }

            await load();
        } catch (err) {
            setError(err.message);
        }
    };

    // =========================
    // CONFIRM AGREEMENT
    // =========================

    const confirm = async () => {
        if (!canConfirm) {
            return;
        }

        try {
            setError("");

            if (rental) {
                await c3Api.rentalConfirm(id);
            } else {
                await c3Api.purchaseConfirm(id);
            }

            await load();
        } catch (err) {
            setError(err.message);
        }
    };

    return (
        <div
            className={
                embedded
                    ? "c3-negotiation-embedded"
                    : "c3-page"
            }
        >
            {!embedded && (
                <div className="c3-header">
                    <div>
                        <span>COMPONENT 3</span>

                        <h1>{title}</h1>

                        <p>
                            Negotiation history, messages and
                            agreement confirmation.
                        </p>
                    </div>

                    <AiAssistant
                        targetType={
                            rental
                                ? "RentalApplication"
                                : "PurchaseOffer"
                        }
                        targetId={id}
                        onWorkflowCompleted={load}
                    />
                </div>
            )}

            {embedded && (
                <div className="c3-ai-row">
                    <div>
                        <p className="page-eyebrow">
                            AI SUPPORT
                        </p>

                        <span>
                            Use the Transaction Negotiation
                            Agent to assist with this
                            transaction.
                        </span>
                    </div>

                    <AiAssistant
                        targetType={
                            rental
                                ? "RentalApplication"
                                : "PurchaseOffer"
                        }
                        targetId={id}
                        onWorkflowCompleted={load}
                    />
                </div>
            )}

            {error && (
                <div className="dashboard-error">
                    {error}
                </div>
            )}

            {transaction && (
                <div className="c3-transaction-state">
                    <span>
                        Transaction: <strong>{transaction.status}</strong>
                    </span>

                    <span>
                        Negotiation: <strong>{transaction.negotiationStatus}</strong>
                    </span>
                </div>
            )}

            <div className="c3-negotiation-grid">
                {/* =========================
                    COUNTER OFFERS
                   ========================= */}

                <section className="c3-workspace-card">
                    <div className="c3-section-title">
                        <div>
                            <p className="page-eyebrow">
                                NEGOTIATION
                            </p>

                            <h2>
                                Counter-offer history
                            </h2>
                        </div>

                        <span className="c3-count">
                            {String(offers.length).padStart(
                                2,
                                "0"
                            )}
                        </span>
                    </div>

                    <div className="c3-offer-history">
                        {offers.length === 0 ? (
                            <div className="c3-inline-empty">
                                No counter-offers yet.
                            </div>
                        ) : (
                            offers.map((offer) => {
                                const ownOffer =
                                    Number(
                                        offer.proposedByUserId
                                    ) === currentUserId;

                                const canAcceptOffer =
                                    offer.status ===
                                    "Pending" &&
                                    negotiationOpen &&
                                    !ownOffer;

                                return (
                                    <div
                                        className="c3-negotiation-offer"
                                        key={offer.id}
                                    >
                                        <div className="c3-negotiation-offer-top">
                                            <strong>
                                                {rental
                                                    ? `LKR ${Number(
                                                        offer.monthlyRent ||
                                                        0
                                                    ).toLocaleString(
                                                        "en-LK"
                                                    )} / month`
                                                    : `LKR ${Number(
                                                        offer.offerAmount ||
                                                        0
                                                    ).toLocaleString(
                                                        "en-LK"
                                                    )}`}
                                            </strong>

                                            <span
                                                className={`c3-status c3-status-${String(
                                                    offer.status
                                                ).toLowerCase()}`}
                                            >
                                                {
                                                    offer.status
                                                }
                                            </span>
                                        </div>

                                        <p>
                                            {offer.conditions ||
                                                "No conditions."}
                                        </p>

                                        {ownOffer &&
                                            offer.status ===
                                            "Pending" && (
                                                <small>
                                                    Your
                                                    counter-offer
                                                </small>
                                            )}

                                        {canAcceptOffer && (
                                            <button
                                                type="button"
                                                className="c3-small-accept"
                                                onClick={() =>
                                                    accept(
                                                        offer
                                                    )
                                                }
                                            >
                                                Accept
                                                counter-offer
                                            </button>
                                        )}
                                    </div>
                                );
                            })
                        )}
                    </div>

                    {canCounter ? (
                        <>
                            <div className="c3-form-divider" />

                            <div className="c3-form-heading">
                                <h3>
                                    Send counter-offer
                                </h3>

                                <p>
                                    Propose revised terms
                                    for this transaction.
                                </p>
                            </div>

                            <div className="c3-form-grid">
                                {rental ? (
                                    <>
                                        <label>
                                            <span>
                                                MONTHLY RENT
                                            </span>

                                            <input
                                                type="number"
                                                placeholder="LKR"
                                                value={rent}
                                                onChange={(
                                                    event
                                                ) =>
                                                    setRent(
                                                        event
                                                            .target
                                                            .value
                                                    )
                                                }
                                            />
                                        </label>

                                        <label>
                                            <span>
                                                MOVE-IN DATE
                                            </span>

                                            <input
                                                type="date"
                                                value={moveIn}
                                                onChange={(
                                                    event
                                                ) =>
                                                    setMoveIn(
                                                        event
                                                            .target
                                                            .value
                                                    )
                                                }
                                            />
                                        </label>

                                        <label>
                                            <span>
                                                DURATION
                                            </span>

                                            <input
                                                type="number"
                                                placeholder="Months"
                                                value={
                                                    duration
                                                }
                                                onChange={(
                                                    event
                                                ) =>
                                                    setDuration(
                                                        event
                                                            .target
                                                            .value
                                                    )
                                                }
                                            />
                                        </label>
                                    </>
                                ) : (
                                    <label className="c3-form-full">
                                        <span>
                                            OFFER AMOUNT
                                        </span>

                                        <input
                                            type="number"
                                            placeholder="LKR"
                                            value={amount}
                                            onChange={(
                                                event
                                            ) =>
                                                setAmount(
                                                    event
                                                        .target
                                                        .value
                                                )
                                            }
                                        />
                                    </label>
                                )}

                                <label className="c3-form-full">
                                    <span>
                                        CONDITIONS
                                    </span>

                                    <textarea
                                        placeholder="Add conditions or notes..."
                                        value={conditions}
                                        onChange={(
                                            event
                                        ) =>
                                            setConditions(
                                                event
                                                    .target
                                                    .value
                                            )
                                        }
                                    />
                                </label>
                            </div>

                            <button
                                type="button"
                                className="c3-primary-button"
                                onClick={counter}
                                disabled={submittingCounter}
                            >
                                {submittingCounter
                                    ? "Sending..."
                                    : "Send counter-offer"}

                                {!submittingCounter && (
                                    <span>→</span>
                                )}
                            </button>
                        </>
                    ) : (
                        <div className="c3-closed-notice">
                            {completed
                                ? "This transaction has been completed."
                                : agreement
                                    ? "Negotiation is closed because the final agreement has been generated."
                                    : transaction?.status ===
                                        "Rejected"
                                        ? "This transaction was rejected."
                                        : "Counter-offers are not available at this stage."}
                        </div>
                    )}
                </section>

                {/* =========================
                    MESSAGES
                   ========================= */}

                <section className="c3-workspace-card">
                    <div className="c3-section-title">
                        <div>
                            <p className="page-eyebrow">
                                CONVERSATION
                            </p>

                            <h2>Messages</h2>
                        </div>

                        <span className="c3-count">
                            {String(
                                messages.length
                            ).padStart(2, "0")}
                        </span>
                    </div>

                    <div className="c3-conversation">
                        {messages.length === 0 ? (
                            <div className="c3-inline-empty">
                                No messages yet.
                            </div>
                        ) : (
                            messages.map((item) => (
                                <div
                                    className="c3-message-item"
                                    key={item.id}
                                >
                                    <div>
                                        <strong>
                                            {
                                                item.senderName
                                            }
                                        </strong>

                                        <small>
                                            {new Date(
                                                item.createdAt
                                            ).toLocaleString()}
                                        </small>
                                    </div>

                                    <p>{item.message}</p>
                                </div>
                            ))
                        )}
                    </div>

                    {canMessage ? (
                        <div className="c3-message-compose-new">
                            <input
                                placeholder="Write a message..."
                                value={message}
                                onChange={(event) =>
                                    setMessage(
                                        event.target.value
                                    )
                                }
                                onKeyDown={(event) => {
                                    if (
                                        event.key ===
                                        "Enter"
                                    ) {
                                        send();
                                    }
                                }}
                            />

                            <button
                                type="button"
                                onClick={send}
                            >
                                Send
                            </button>
                        </div>
                    ) : (
                        <div className="c3-closed-notice">
                            {negotiationClosed ||
                                agreement ||
                                completed
                                ? "Conversation is read-only because negotiation is closed."
                                : "Messages become available once negotiation is open."}
                        </div>
                    )}
                </section>
            </div>

            {/* =========================
                AGREEMENT
               ========================= */}

            {agreement && (
                <section className="c3-workspace-card c3-agreement-new">
                    <div className="c3-section-title">
                        <div>
                            <p className="page-eyebrow">
                                FINAL TERMS
                            </p>

                            <h2>Agreement</h2>
                        </div>

                        <span
                            className={`c3-status c3-status-${String(
                                agreement.status
                            ).toLowerCase()}`}
                        >
                            {agreement.status}
                        </span>
                    </div>

                    <div className="c3-agreement-price">
                        <span>
                            {rental
                                ? "FINAL MONTHLY RENT"
                                : "FINAL PURCHASE PRICE"}
                        </span>

                        <strong>
                            LKR{" "}
                            {Number(
                                rental
                                    ? agreement.finalMonthlyRent
                                    : agreement.finalPurchasePrice
                            ).toLocaleString("en-LK")}
                        </strong>
                    </div>

                    <div className="c3-agreement-terms">
                        <div>
                            <span>
                                {rental
                                    ? "TENANT OBLIGATION"
                                    : "BUYER OBLIGATION"}
                            </span>

                            <p>
                                {rental
                                    ? agreement.tenantObligation
                                    : agreement.buyerObligation}
                            </p>
                        </div>

                        <div>
                            <span>
                                {rental
                                    ? "OWNER OBLIGATION"
                                    : "SELLER OBLIGATION"}
                            </span>

                            <p>
                                {rental
                                    ? agreement.ownerObligation
                                    : agreement.sellerObligation}
                            </p>
                        </div>

                        <div>
                            <span>
                                PENALTY / REMEDY TERMS
                            </span>

                            <p>
                                {agreement.penaltyTerms}
                            </p>
                        </div>
                    </div>

                    <div className="c3-confirmation-row">
                        <span>
                            Buyer / Tenant
                            <strong>
                                {agreement.buyerConfirmed
                                    ? "Confirmed"
                                    : "Waiting"}
                            </strong>
                        </span>

                        <span>
                            Seller / Owner
                            <strong>
                                {agreement.sellerConfirmed
                                    ? "Confirmed"
                                    : "Waiting"}
                            </strong>
                        </span>
                    </div>

                    {canConfirm && (
                        <button
                            type="button"
                            className="c3-primary-button"
                            onClick={confirm}
                        >
                            Confirm agreement
                            <span>→</span>
                        </button>
                    )}

                    {alreadyConfirmed &&
                        !completed && (
                            <div className="c3-closed-notice">
                                You have confirmed this
                                agreement. Waiting for the
                                other party to confirm.
                            </div>
                        )}

                    {completed && (
                        <div className="c3-closed-notice">
                            Agreement completed. Both
                            parties have confirmed the
                            final terms.
                        </div>
                    )}
                </section>
            )}
        </div>
    );
}