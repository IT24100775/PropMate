import json
import os
from typing import Any

import psycopg
from psycopg.rows import dict_row


DATABASE_URL = os.getenv("DATABASE_URL")

if not DATABASE_URL:
    raise RuntimeError("DATABASE_URL environment variable is not configured.")


def connect():
    return psycopg.connect(
        DATABASE_URL,
        row_factory=dict_row
    )


def init_db():
    with connect() as con:
        with con.cursor() as cur:
            cur.execute("""
                CREATE TABLE IF NOT EXISTS agent_workflows (
                    id BIGSERIAL PRIMARY KEY,
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
        with con.cursor() as cur:
            cur.execute("""
                INSERT INTO agent_workflows (
                    workflow_id,
                    initiated_by_user_id,
                    initiated_by_role,
                    objective,
                    target_type,
                    target_id,
                    status,
                    plan_json,
                    steps_json,
                    tool_calls_json,
                    approvals_json,
                    final_outcome_json,
                    error,
                    created_at,
                    updated_at,
                    completed_at
                )
                VALUES (
                    %s, %s, %s, %s,
                    %s, %s, %s, %s,
                    %s, %s, %s, %s,
                    %s, %s, %s, %s
                )
                ON CONFLICT (workflow_id)
                DO UPDATE SET
                    status = EXCLUDED.status,
                    plan_json = EXCLUDED.plan_json,
                    steps_json = EXCLUDED.steps_json,
                    tool_calls_json = EXCLUDED.tool_calls_json,
                    approvals_json = EXCLUDED.approvals_json,
                    final_outcome_json = EXCLUDED.final_outcome_json,
                    error = EXCLUDED.error,
                    updated_at = EXCLUDED.updated_at,
                    completed_at = EXCLUDED.completed_at
            """, (
                record["workflow_id"],
                record["initiated_by_user_id"],
                record["initiated_by_role"],
                record["objective"],
                record["target_type"],
                record["target_id"],
                record["status"],
                json.dumps(record.get("plan")),
                json.dumps(record.get("steps", [])),
                json.dumps(record.get("tool_calls", [])),
                json.dumps(record.get("approvals", [])),
                json.dumps(record.get("final_outcome")),
                record.get("error"),
                record["created_at"],
                record["updated_at"],
                record.get("completed_at")
            ))


def get(workflow_id: str):
    with connect() as con:
        with con.cursor() as cur:
            cur.execute(
                """
                SELECT *
                FROM agent_workflows
                WHERE workflow_id = %s
                """,
                (workflow_id,)
            )

            return cur.fetchone()


def list_for_user(user_id: int):
    with connect() as con:
        with con.cursor() as cur:
            cur.execute(
                """
                SELECT *
                FROM agent_workflows
                WHERE initiated_by_user_id = %s
                ORDER BY created_at DESC
                """,
                (user_id,)
            )

            return cur.fetchall()