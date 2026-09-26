import { useCallback, useEffect, useState } from "react";
import { c3Api } from "../services/api";
import AiAssistant from "./AiAssistant";

export default function NegotiationPanel({ type, id, title }) {
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

  const load = useCallback(async () => {
    try {
      const [nextOffers, nextMessages, nextAgreement] = await Promise.all([
        rental ? c3Api.rentalOffers(id) : c3Api.purchaseOffers(id),
        rental ? c3Api.rentalMessages(id) : c3Api.purchaseMessages(id),
        rental ? c3Api.rentalAgreement(id) : c3Api.purchaseAgreement(id),
      ]);
      setOffers(nextOffers);
      setMessages(nextMessages);
      setAgreement(nextAgreement);
      setError("");
    } catch (loadError) {
      setError(loadError.message);
    }
  }, [id, rental]);

  useEffect(() => {
    const timer = window.setTimeout(() => void load(), 0);
    return () => window.clearTimeout(timer);
  }, [load]);

  const submitCounter = async () => {
    try {
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
      setConditions("");
      await load();
    } catch (actionError) {
      setError(actionError.message);
    }
  };

  const sendMessage = async () => {
    if (!message.trim()) return;
    try {
      if (rental) await c3Api.rentalMessage(id, { message: message.trim() });
      else await c3Api.purchaseMessage(id, { message: message.trim() });
      setMessage("");
      await load();
    } catch (actionError) {
      setError(actionError.message);
    }
  };

  const acceptOffer = async (offer) => {
    try {
      if (rental) await c3Api.rentalAcceptCounter(id, offer.id);
      else await c3Api.purchaseAcceptCounter(id, offer.id);
      await load();
    } catch (actionError) {
      setError(actionError.message);
    }
  };

  const confirmAgreement = async () => {
    try {
      if (rental) await c3Api.rentalConfirm(id);
      else await c3Api.purchaseConfirm(id);
      await load();
    } catch (actionError) {
      setError(actionError.message);
    }
  };

  return (
    <div className="c3-page">
      <div className="c3-header">
        <div>
          <span>COMPONENT 3</span>
          <h1>{title}</h1>
          <p>Negotiation history, messages and agreement confirmation.</p>
        </div>
        <AiAssistant
          targetType={rental ? "RentalApplication" : "PurchaseOffer"}
          targetId={id}
        />
      </div>
      {error && <div className="c3-error">{error}</div>}
      <div className="c3-grid">
        <section className="c3-card">
          <h2>Counter-offer history</h2>
          {offers.length === 0 ? (
            <p>No counter-offers yet.</p>
          ) : offers.map((offer) => (
            <div className="c3-offer" key={offer.id}>
              <div>
                <b>{rental ? `LKR ${offer.monthlyRent} / month` : `LKR ${offer.offerAmount}`}</b>
                <span>{offer.status}</span>
              </div>
              <p>{offer.conditions || "No conditions."}</p>
              {offer.status === "Pending" && (
                <button onClick={() => void acceptOffer(offer)}>Accept</button>
              )}
            </div>
          ))}
          <h3>Send counter-offer</h3>
          {rental ? (
            <>
              <input placeholder="Monthly rent" value={rent} onChange={(event) => setRent(event.target.value)} />
              <input type="date" value={moveIn} onChange={(event) => setMoveIn(event.target.value)} />
              <input placeholder="Duration (months)" value={duration} onChange={(event) => setDuration(event.target.value)} />
            </>
          ) : (
            <input placeholder="Offer amount" value={amount} onChange={(event) => setAmount(event.target.value)} />
          )}
          <textarea placeholder="Conditions" value={conditions} onChange={(event) => setConditions(event.target.value)} />
          <button onClick={() => void submitCounter()}>Send counter-offer</button>
        </section>
        <section className="c3-card">
          <h2>Messages</h2>
          <div className="c3-messages">
            {messages.map((item) => (
              <div key={item.id}>
                <b>{item.senderName}</b>
                <p>{item.message}</p>
                <small>{new Date(item.createdAt).toLocaleString()}</small>
              </div>
            ))}
          </div>
          <div className="c3-message-compose">
            <input placeholder="Write a message" value={message} onChange={(event) => setMessage(event.target.value)} />
            <button onClick={() => void sendMessage()}>Send</button>
          </div>
        </section>
      </div>
      {agreement && (
        <section className="c3-card c3-agreement">
          <h2>Agreement</h2>
          <p><b>Status:</b> {agreement.status}</p>
          <p><b>{rental ? "Final monthly rent" : "Final purchase price"}:</b> LKR {rental ? agreement.finalMonthlyRent : agreement.finalPurchasePrice}</p>
          <p>{rental ? agreement.tenantObligation : agreement.buyerObligation}</p>
          <p>{rental ? agreement.ownerObligation : agreement.sellerObligation}</p>
          <p><b>Penalty / remedy terms:</b> {agreement.penaltyTerms}</p>
          <div className="c3-confirm">
            <span>Buyer/Tenant: {agreement.buyerConfirmed ? "Confirmed" : "Waiting"}</span>
            <span>Seller/Owner: {agreement.sellerConfirmed ? "Confirmed" : "Waiting"}</span>
          </div>
          {(!agreement.buyerConfirmed || !agreement.sellerConfirmed) && (
            <button onClick={() => void confirmAgreement()}>Confirm Agreement</button>
          )}
        </section>
      )}
    </div>
  );
}
