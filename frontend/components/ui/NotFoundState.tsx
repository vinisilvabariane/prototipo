import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import { inkSoft, lineStrong } from "@/lib/tokens";
import LogoMark from "./LogoMark";

type Action = { href: string; label: string };

type Props = {
  title: string;
  message: string;
  actions: Action[];
  fullPage?: boolean;
};

export default function NotFoundState({ title, message, actions, fullPage }: Props) {
  return (
    <Box
      sx={{
        flex: 1,
        display: "grid",
        placeItems: "center",
        minHeight: fullPage ? "100vh" : "50vh",
        p: 3,
        textAlign: "center",
      }}
    >
      <Stack spacing={2} sx={{ alignItems: "center", maxWidth: 440 }}>
        {fullPage && <LogoMark size={28} />}
        <Typography sx={{ fontWeight: 600, fontSize: "5rem", lineHeight: 1, letterSpacing: "-0.05em", color: lineStrong }}>
          404
        </Typography>
        <Typography variant="h4" component="h1">
          {title}
        </Typography>
        <Typography sx={{ color: inkSoft }}>{message}</Typography>
        <Stack direction="row" spacing={1.5} sx={{ pt: 1 }}>
          {actions.map((action, index) => (
            <Button key={action.href} href={action.href} variant={index === 0 ? "contained" : "outlined"}>
              {action.label}
            </Button>
          ))}
        </Stack>
      </Stack>
    </Box>
  );
}
