"use client";

import { useSyncExternalStore } from "react";
import Box from "@mui/material/Box";
import { monoFont } from "@/lib/tokens";

const formatter = new Intl.DateTimeFormat("pt-BR", {
  weekday: "short",
  day: "2-digit",
  month: "short",
  hour: "2-digit",
  minute: "2-digit",
});

function subscribe(onChange: () => void) {
  const id = setInterval(onChange, 5000);
  return () => clearInterval(id);
}

const currentMinute = () => Math.floor(Date.now() / 60000);
const onServer = () => null;

export default function LiveClock() {
  const minute = useSyncExternalStore(subscribe, currentMinute, onServer);

  return (
    <Box
      component="time"
      sx={{
        display: { xs: "none", xl: "block" },
        whiteSpace: "nowrap",
        minWidth: 150,
        textAlign: "right",
        fontFamily: monoFont,
        fontSize: "0.8rem",
        letterSpacing: "0.02em",
        color: "text.secondary",
      }}
    >
      {minute === null ? "—" : formatter.format(minute * 60000)}
    </Box>
  );
}
