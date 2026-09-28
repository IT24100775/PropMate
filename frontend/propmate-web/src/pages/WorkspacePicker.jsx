import { Link } from "react-router-dom";
import "./WorkspacePicker.css";

const workspaces = [
  { path: "/tenant", number: "01", name: "Tenant", description: "Browse properties, apply to rent, make offers, and submit maintenance requests." },
  { path: "/owner", number: "02", name: "Owner / Seller", description: "Manage property listings, applications, offers, and agreements." },
  { path: "/maintenance", number: "03", name: "Property Manager", description: "Review maintenance requests, AI recommendations, technicians, expenses, and history." },
  { path: "/admin", number: "04", name: "Administrator", description: "Review property listings and transaction activity." },
];

export default function WorkspacePicker() {
  return (
    <main className="workspace-picker">
      <header><span>PROPMATE</span><p>DEMO WORKSPACES</p><h1>Choose a workspace</h1></header>
      <section className="workspace-list">
        {workspaces.map((workspace) => (
          <Link className="workspace-option" to={workspace.path} key={workspace.path}>
            <span>{workspace.number}</span>
            <div><h2>{workspace.name}</h2><p>{workspace.description}</p></div>
            <b aria-hidden="true">→</b>
          </Link>
        ))}
      </section>
      <footer>Authentication is disabled. All visitors use shared demo data.</footer>
    </main>
  );
}
