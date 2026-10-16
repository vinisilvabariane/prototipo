"use client";

import { useSyncExternalStore } from "react";
import Box from "@mui/material/Box";
import { inkSoft } from "@/lib/tokens";

const dateFormatter = new Intl.DateTimeFormat("pt-BR", { weekday: "long", day: "numeric", month: "long" });
const timeFormatter = new Intl.DateTimeFormat("pt-BR", { hour: "2-digit", minute: "2-digit" });

function subscribe(onChange: () => void) {
  const id = setInterval(onChange, 5000);
  return () => clearInterval(id);
}

const currentMinute = () => Math.floor(Date.now() / 60000);
const onServer = () => null;

const capitalize = (text: string) => text.charAt(0).toUpperCase() + text.slice(1);

export default function LiveClock() {
  const minute = useSyncExternalStore(subscribe, currentMinute, onServer);
  const now = minute === null ? null : new Date(minute * 60000);

  return (
    <Box component="time" sx={{ display: "block", textAlign: { md: "right" }, whiteSpace: "nowrap" }}>
      <Box sx={{ fontWeight: 600, fontSize: "1.9rem", lineHeight: 1, letterSpacing: "-0.03em", fontVariantNumeric: "tabular-nums" }}>
        {now ? timeFormatter.format(now) : "--:--"}
      </Box>
      <Box sx={{ mt: 0.5, fontSize: "0.85rem", color: inkSoft }}>{now ? capitalize(dateFormatter.format(now)) : ""}</Box>
    </Box>
  );
}
