// Top navbar — shared across all portals. Shows notification bell and user profile on the right. Clicking profile opens an ActionCard with a logout option.

import { ActionCard } from "./ActionCard";

export type NavbarProps = {
  onLogout: () => void;
  userName?: string;
};

function getGreeting(): string {
  const hour = new Date().getHours();
  if (hour < 12) return "Good morning";
  if (hour < 17) return "Good afternoon";
  return "Good evening";
}

export function Navbar({ onLogout, userName = "User" }: NavbarProps) {
  const initial = userName.trim().charAt(0).toUpperCase() || "U";
  const firstName = userName.trim().split(" ")[0];

  return (
    <nav className="fixed left-0 right-0 top-0 z-40 h-16 border-b border-gray-200 bg-white shadow-sm">
      <div className="flex h-full items-center justify-between px-6">
        <span className="text-xl font-bold text-blue-600">Telebill</span>
        <span className="text-base text-gray-500">
          {getGreeting()}, <span className="font-semibold text-gray-800">{firstName}</span>
        </span>
        <div className="flex items-center gap-4">
        <ActionCard
          trigger={
            <div className="flex h-9 w-9 cursor-pointer items-center justify-center rounded-full bg-blue-600 text-sm font-semibold text-white transition-colors hover:bg-blue-700">
              {initial}
            </div>
          }
          items={[
            {
              label: "Logout",
              onClick: onLogout,
              variant: "danger",
            },
          ]}
        />
        </div>
      </div>
    </nav>
  );
}

export default Navbar;
