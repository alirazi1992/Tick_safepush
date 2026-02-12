"use client";

import { useEffect, useState } from "react";
import Image from "next/image";
import { useRouter } from "next/navigation";
import { LogIn } from "lucide-react";
import { useAuth } from "@/lib/auth-context";
import { Button } from "@/components/ui/button";

const ENABLE_LEGACY_LOGIN = process.env.NEXT_PUBLIC_ENABLE_LEGACY_LOGIN === "true";

export default function LoginPage() {
  const router = useRouter();
  const { user, isLoading, refreshSession, isExternalMode, isLanMode, startExternalLogin } = useAuth();
  const [checkingLan, setCheckingLan] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (user) {
      router.replace("/");
    }
  }, [user, router]);

  useEffect(() => {
    if (!isLoading && isLanMode && !isExternalMode) {
      setCheckingLan(true);
      setError(null);
      refreshSession()
        .catch((err: any) => {
          setError(err?.message || "Sign-in failed.");
        })
        .finally(() => setCheckingLan(false));
    }
  }, [isLoading, isLanMode, isExternalMode, refreshSession]);

  return (
    <div className="min-h-screen flex bg-background text-foreground relative overflow-x-hidden">
      <div className="absolute inset-0 z-0">
        <div
          className="absolute inset-0 bg-cover bg-center bg-no-repeat"
          style={{
            backgroundImage: "url('/container-ship-bg.png'), url('/placeholder.jpg')",
          }}
          aria-hidden
        />
        <div className="absolute inset-0 bg-gradient-to-br from-background/90 via-background/80 to-background/70" />
      </div>

      <div className="relative z-10 flex-1 flex items-center justify-center p-6">
        <div className="w-full max-w-md text-center space-y-6">
          <div className="text-center mb-8 flex items-center justify-center gap-3">
            <h1 className="text-4xl font-extrabold tracking-tight drop-shadow md:text-5xl">AsiaTik</h1>
            <Image src="/checkmark.png" alt="" width={40} height={40} className="h-8 w-8 md:h-10 md:w-10 shrink-0" />
          </div>

          {isExternalMode ? (
            <>
              <p className="text-sm text-muted-foreground">برای ورود، از احراز هویت سازمانی استفاده کنید.</p>
              <Button
                type="button"
                className="w-full"
                onClick={() => void startExternalLogin()}
                disabled={isLoading}
              >
                <span className="inline-flex items-center gap-2">
                  <LogIn className="w-4 h-4" />
                  Sign in with Company SSO
                </span>
              </Button>
            </>
          ) : (
            <>
              <p className="text-sm text-muted-foreground">
                {checkingLan ? "Signing you in..." : "Attempting automatic intranet sign-in..."}
              </p>
              {error && <p className="text-sm text-destructive">{error}</p>}
              {!checkingLan && (
                <Button
                  type="button"
                  variant="outline"
                  className="w-full"
                  onClick={() => void startExternalLogin()}
                >
                  Sign in with Company SSO
                </Button>
              )}
            </>
          )}

          {ENABLE_LEGACY_LOGIN && (
            <p className="text-xs text-muted-foreground">
              Legacy username/password login is enabled for local development.
            </p>
          )}
        </div>
      </div>
    </div>
  );
}
