import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { c3Api } from "../services/api";

function AdminLookup({ type }) {
  const [id, setId] = useState("");
  const [data, setData] = useState(null);
  const [error, setError] = useState("");

  const lookup = async () => {
    try {
      setData(await (type === "rental" ? c3Api.rentalGet(id) : c3Api.purchaseGet(id)));
      setError("");
    } catch (lookupError) {
      setError(lookupError.message);
    }
  };

  return (
    <>
      <input placeholder="Transaction ID" value={id} onChange={(event) => setId(event.target.value)} />
      <button onClick={() => void lookup()}>View</button>
      {error && <div className="c3-error">{error}</div>}
      {data && <pre>{JSON.stringify(data, null, 2)}</pre>}
    </>
  );
}

export default function TransactionInbox({ type, admin = false }) {
  const rental = type === "rental";
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    let live = true;
    const load = async () => {
      try {
        const result = admin
          ? []
          : rental
            ? await c3Api.rentalOwner()
            : await c3Api.purchaseOwner();
        if (live) setItems(result);
      } catch (loadError) {
        if (live) setError(loadError.message);
      } finally {
        if (live) setLoading(false);
      }
    };
    void load();
    return () => {
      live = false;
    };
  }, [type, admin, rental]);

  if (admin) {
    return (
      <div className="c3-page">
        <h1>Component 3: Admin view</h1>
        <p>Admin access is view-only. Enter a transaction ID to inspect it.</p>
        <div className="c3-card"><AdminLookup type={type} /></div>
      </div>
    );
  }

  const updateStatus = async (item, accepted) => {
    try {
      await (rental
        ? accepted ? c3Api.rentalAccept(item.id) : c3Api.rentalReject(item.id)
        : accepted ? c3Api.purchaseAccept(item.id) : c3Api.purchaseReject(item.id));
      setItems((current) => current.map((row) =>
        row.id === item.id ? { ...row, status: accepted ? "Accepted" : "Rejected" } : row,
      ));
    } catch (actionError) {
      setError(actionError.message);
    }
  };

  return (
    <div className="c3-page">
      <div className="c3-header">
        <div>
          <span>COMPONENT 3</span>
          <h1>{rental ? "Rental applications" : "Purchase offers"}</h1>
          <p>Review submissions and open negotiations for your listings.</p>
        </div>
      </div>
      {error && <div className="c3-error">{error}</div>}
      {loading ? <p>Loading…</p> : items.length === 0 ? (
        <div className="c3-card">No transactions found.</div>
      ) : (
        <div className="c3-list">
          {items.map((item) => (
            <article className="c3-card" key={item.id}>
              <div className="c3-card-top">
                <div><span>{item.propertyTitle}</span><h2>#{item.id} · {rental ? item.tenantName : item.buyerName}</h2></div>
                <b>{item.status}</b>
              </div>
              {rental
                ? <p>Income: LKR {item.monthlyIncome} · Occupants: {item.occupants} · Move-in: {item.preferredMoveInDate}</p>
                : <p>Offer: LKR {item.offerAmount}</p>}
              <p>{item.message || item.conditions || "No additional message."}</p>
              <div className="c3-actions">
                <Link to={`/owner/component3/${type}/${item.id}`}>Open negotiation</Link>
                <button onClick={() => void updateStatus(item, true)}>Accept</button>
                <button onClick={() => void updateStatus(item, false)}>Reject</button>
              </div>
            </article>
          ))}
        </div>
      )}
    </div>
  );
}
