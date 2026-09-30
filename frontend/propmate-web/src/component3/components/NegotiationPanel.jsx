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

    const [offers, setOffers] = useState([]);
    const [messages, setMessages] = useState([]);
    const [agreement, setAgreement] = useState(null);

    const [message, setMessage] = useState("");
    const [amount, setAmount] = useState("");
    const [rent, setRent] = useState("");
    const [moveIn, setMoveIn] = useState("");
    const [duration, setDuration] = useState("12");
    const [conditions, setConditions] = useState("");

    const [error, setError] = useState("");

    const load = async () => {
        try {
            setError("");

            const [offerData, messageData, agreementData] =
                await Promise.all([
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

    const counter = async () => {
        try {
            setError("");

            if (rental) {
                await c3Api.rentalCounter(id, {
                    monthlyRent: Number(rent),
                    moveInDate: moveIn,
                    durationMonths: Number(duration),
                    conditions,
                });
            } else {
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
        }
    };

    const send = async () => {
        if (!message.trim()) {
            return;
        }

        try {
            setError("");

            if (rental) {
                await c3Api.rentalMessage(id, { message });
            } else {
                await c3Api.purchaseMessage(id, { message });
            }

            setMessage("");
            await load();
        } catch (err) {
            setError(err.message);
        }
    };

    const accept = async (offer) => {
        try {
            setError("");

            if (rental) {
                await c3Api.rentalAcceptCounter(id, offer.id);
            } else {
                await c3Api.purchaseAcceptCounter(id, offer.id);
            }

            await load();
        } catch (err) {
            setError(err.message);
        }
    };

    const confirm = async () => {
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
        <div className={embedded ? "c3-negotiation-embedded" : "c3-page"}>
            {!embedded && (
                <div className="c3-header">
                    <div>
                        <span>COMPONENT 3</span>
                        <h1>{title}</h1>
                        <p>
                            Negotiation history, messages and agreement confirmation.
                        </p>
                    </div>

                    <AiAssistant
                        targetType={
                            rental ? "RentalApplication" : "PurchaseOffer"
                        }
                        targetId={id}
                    />
                </div>
            )}

            {embedded && (
                <div className="c3-ai-row">
                    <div>
                        <p className="page-eyebrow">AI SUPPORT</p>
                        <span>
                            Use the Transaction Negotiation Agent to assist with
                            this transaction.
                        </span>
                    </div>

                    <AiAssistant
                        targetType={
                            rental ? "RentalApplication" : "PurchaseOffer"
                        }
                        targetId={id}
                    />
                </div>
            )}

            {error && (
                <div className="dashboard-error">
                    {error}
                </div>
            )}

            <div className="c3-negotiation-grid">
                <section className="c3-workspace-card">
                    <div className="c3-section-title">
                        <div>
                            <p className="page-eyebrow">NEGOTIATION</p>
                            <h2>Counter-offer history</h2>
                        </div>

                        <span className="c3-count">
                            {String(offers.length).padStart(2, "0")}
                        </span>
                    </div>

                    <div className="c3-offer-history">
                        {offers.length === 0 ? (
                            <div className="c3-inline-empty">
                                No counter-offers yet.
                            </div>
                        ) : (
                            offers.map((offer) => (
                                <div className="c3-negotiation-offer" key={offer.id}>
                                    <div className="c3-negotiation-offer-top">
                                        <strong>
                                            {rental
                                                ? `LKR ${Number(
                                                    offer.monthlyRent || 0
                                                ).toLocaleString("en-LK")} / month`
                                                : `LKR ${Number(
                                                    offer.offerAmount || 0
                                                ).toLocaleString("en-LK")}`}
                                        </strong>

                                        <span
                                            className={`c3-status c3-status-${String(
                                                offer.status
                                            ).toLowerCase()}`}
                                        >
                                            {offer.status}
                                        </span>
                                    </div>

                                    <p>
                                        {offer.conditions || "No conditions."}
                                    </p>

                                    {offer.status === "Pending" && (
                                        <button
                                            type="button"
                                            className="c3-small-accept"
                                            onClick={() => accept(offer)}
                                        >
                                            Accept counter-offer
                                        </button>
                                    )}
                                </div>
                            ))
                        )}
                    </div>

                    <div className="c3-form-divider" />

                    <div className="c3-form-heading">
                        <h3>Send counter-offer</h3>
                        <p>
                            Propose revised terms for this transaction.
                        </p>
                    </div>

                    <div className="c3-form-grid">
                        {rental ? (
                            <>
                                <label>
                                    <span>MONTHLY RENT</span>
                                    <input
                                        type="number"
                                        placeholder="LKR"
                                        value={rent}
                                        onChange={(event) =>
                                            setRent(event.target.value)
                                        }
                                    />
                                </label>

                                <label>
                                    <span>MOVE-IN DATE</span>
                                    <input
                                        type="date"
                                        value={moveIn}
                                        onChange={(event) =>
                                            setMoveIn(event.target.value)
                                        }
                                    />
                                </label>

                                <label>
                                    <span>DURATION</span>
                                    <input
                                        type="number"
                                        placeholder="Months"
                                        value={duration}
                                        onChange={(event) =>
                                            setDuration(event.target.value)
                                        }
                                    />
                                </label>
                            </>
                        ) : (
                            <label className="c3-form-full">
                                <span>OFFER AMOUNT</span>
                                <input
                                    type="number"
                                    placeholder="LKR"
                                    value={amount}
                                    onChange={(event) =>
                                        setAmount(event.target.value)
                                    }
                                />
                            </label>
                        )}

                        <label className="c3-form-full">
                            <span>CONDITIONS</span>
                            <textarea
                                placeholder="Add conditions or notes..."
                                value={conditions}
                                onChange={(event) =>
                                    setConditions(event.target.value)
                                }
                            />
                        </label>
                    </div>

                    <button
                        type="button"
                        className="c3-primary-button"
                        onClick={counter}
                    >
                        Send counter-offer
                        <span>→</span>
                    </button>
                </section>

                <section className="c3-workspace-card">
                    <div className="c3-section-title">
                        <div>
                            <p className="page-eyebrow">CONVERSATION</p>
                            <h2>Messages</h2>
                        </div>

                        <span className="c3-count">
                            {String(messages.length).padStart(2, "0")}
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
                                        <strong>{item.senderName}</strong>
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

                    <div className="c3-message-compose-new">
                        <input
                            placeholder="Write a message..."
                            value={message}
                            onChange={(event) =>
                                setMessage(event.target.value)
                            }
                            onKeyDown={(event) => {
                                if (event.key === "Enter") {
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
                </section>
            </div>

            {agreement && (
                <section className="c3-workspace-card c3-agreement-new">
                    <div className="c3-section-title">
                        <div>
                            <p className="page-eyebrow">FINAL TERMS</p>
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
                            <span>PENALTY / REMEDY TERMS</span>
                            <p>{agreement.penaltyTerms}</p>
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

                    {(!agreement.buyerConfirmed ||
                        !agreement.sellerConfirmed) && (
                            <button
                                type="button"
                                className="c3-primary-button"
                                onClick={confirm}
                            >
                                Confirm agreement
                                <span>→</span>
                            </button>
                        )}
                </section>
            )}
        </div>
    );
}