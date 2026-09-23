import json
import sqlite3
from pathlib import Path
from typing import Any

DB_PATH = Path(__file__).resolve().parents[2] / "data" / "agent_workflows.db"
DB_PATH.parent.mkdir(parents=True, exist_ok=True)


def connect():
    con = sqlite3.connect(DB_PATH)
    con.row_factory = sqlite3.Row
    return con


def init_db():
    with connect() as con:
        con.execute("""
        CREATE TABLE IF NOT EXISTS workflows (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            workflow_id TEXT UNIQUE NOT NULL,
            initiated_by_user_id INTEGER NOT NULL,
            initiated_by_role TEXT NOT NULL,
            objective TEXT NOT NULL,
            target_type TEXT NOT NULL,
            target_id INTEGER NOT NULL,
            status TEXT NOT NULL,
            plan_json TEXT,
            steps_json TEXT NOT NULL,
            tool_calls_json TEXT NOT NULL,
            approvals_json TEXT NOT NULL,
            final_outcome_json TEXT,
            error TEXT,
            created_at TEXT NOT NULL,
            updated_at TEXT NOT NULL,
            completed_at TEXT
        )
        """)


def save(record: dict[str, Any]):
    with connect() as con:
        con.execute("""
        INSERT INTO workflows (
            workflow_id, initiated_by_user_id, initiated_by_role, objective,
            target_type, target_id, status, plan_json, steps_json,
            tool_calls_json, approvals_json, final_outcome_json, error,
            created_at, updated_at, completed_at
        ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
        ON CONFLICT(workflow_id) DO UPDATE SET
            status=excluded.status,
            plan_json=excluded.plan_json,
            steps_json=excluded.steps_json,
            tool_calls_json=excluded.tool_calls_json,
            approvals_json=excluded.approvals_json,
            final_outcome_json=excluded.final_outcome_json,
            error=excluded.error,
            updated_at=excluded.updated_at,
            completed_at=excluded.completed_at
        """, (
            record["workflow_id"], record["initiated_by_user_id"], record["initiated_by_role"],
            record["objective"], record["target_type"], record["target_id"], record["status"],
            json.dumps(record.get("plan")), json.dumps(record.get("steps", [])),
            json.dumps(record.get("tool_calls", [])), json.dumps(record.get("approvals", [])),
            json.dumps(record.get("final_outcome")), record.get("error"), record["created_at"],
            record["updated_at"], record.get("completed_at")
        ))


def get(workflow_id: str):
    with connect() as con:
        row = con.execute("SELECT * FROM workflows WHERE workflow_id = ?", (workflow_id,)).fetchone()
        return dict(row) if row else None


def list_for_user(user_id: int):
    with connect() as con:
        return [dict(r) for r in con.execute(
            "SELECT * FROM workflows WHERE initiated_by_user_id = ? ORDER BY created_at DESC", (user_id,)
        ).fetchall()]
