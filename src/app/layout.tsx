import { AuthProvider } from '@/contexts/AuthContext';
import { LocationProvider } from '@/contexts/LocationContext';
import LocationSelector from '@/components/LocationSelector';
import './globals.css';

type RootLayoutProps = Readonly<{
  children: React.ReactNode;
}>;

export default function RootLayout({
  children,
}: RootLayoutProps) {
  return (
    <html lang="en">
      <body>
        <AuthProvider>
          <LocationProvider>
            <div className="min-h-screen bg-gray-100">
              <nav className="bg-white shadow-sm">
                <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 py-4 flex justify-between items-center">
                  <h1 className="text-xl font-semibold text-gray-900">Casino Royale</h1>
                  <LocationSelector />
                </div>
              </nav>
              <main className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 py-8">
                {children}
              </main>
            </div>
          </LocationProvider>
        </AuthProvider>
      </body>
    </html>
  );
}
