#!/usr/bin/env python3
"""Extract coverage metrics from Cobertura XML and publish GitHub outputs."""
from __future__ import annotations

import glob
import os
import sys
import xml.etree.ElementTree as ET


def main() -> None:
    paths = glob.glob("TestResults/**/coverage.cobertura.xml", recursive=True)
    if not paths:
        raise SystemExit("Arquivo de cobertura não encontrado.")

    tree = ET.parse(paths[0])
    root = tree.getroot()
    line_rate = root.get("line-rate") or "0"

    try:
        coverage = round(float(line_rate) * 100, 2)
    except ValueError as exc:  # pragma: no cover - defensive
        raise SystemExit("Valor inválido de cobertura no relatório.") from exc

    display = f"{coverage:.2f}".rstrip("0").rstrip(".")

    if coverage >= 90:
        color = "brightgreen"
    elif coverage >= 75:
        color = "green"
    elif coverage >= 60:
        color = "yellow"
    else:
        color = "red"

    badge_label = display.replace(".", "%2E") + "%25"

    output_path = os.environ.get("GITHUB_OUTPUT")
    if not output_path:
        print(f"percent={display}")
        print(f"badge_label={badge_label}")
        print(f"badge_color={color}")
        return

    with open(output_path, "a", encoding="utf-8") as handle:
        handle.write(f"percent={display}\n")
        handle.write(f"badge_label={badge_label}\n")
        handle.write(f"badge_color={color}\n")


if __name__ == "__main__":
    try:
        main()
    except Exception as error:  # pragma: no cover - surfaces clear failure message
        print(error, file=sys.stderr)
        raise
