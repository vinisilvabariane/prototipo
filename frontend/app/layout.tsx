import type { Metadata } from "next";
import { Google_Sans } from "next/font/google";
import InitColorSchemeScript from "@mui/material/InitColorSchemeScript";
import { AppRouterCacheProvider } from "@mui/material-nextjs/v16-appRouter";
import Providers from "@/components/providers/Providers";
import { prefsBootScript } from "@/lib/prefs/bootScript";
import "./globals.css";

const googleSans = Google_Sans({
  variable: "--font-google-sans",
  subsets: ["latin"],
  weight: ["400", "500", "600", "700"],
});

export const metadata: Metadata = {
  title: { default: "MeOne", template: "%s · MeOne" },
  description: "Controle de acesso",
};

export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html
      lang="pt-BR"
      className={`${googleSans.variable} h-full antialiased`}
      suppressHydrationWarning
    >
      <body className="min-h-full flex flex-col">
        <InitColorSchemeScript attribute="data-mui-color-scheme" defaultMode="light" />
        <script dangerouslySetInnerHTML={{ __html: prefsBootScript }} />
        <AppRouterCacheProvider>
          <Providers>{children}</Providers>
        </AppRouterCacheProvider>
      </body>
    </html>
  );
}
