'use client';

import { clientEnv } from '@/lib/env';
import { useEffect, useState } from 'react';
import { useLocation } from '@/contexts/LocationContext';

async function getDailyMenu(locationId: string) {
  const res = await fetch(`${clientEnv.apiUrl}/api/locations/${locationId}/menu/today`, {
    next: { revalidate: 60 }, // Revalidate every minute
  });

  if (!res.ok) throw new Error('Failed to fetch menu');
  return res.json();
}

interface MenuItem {
  id: string;
  name: string;
  description: string;
  type: string;
  isSpecialOffer: boolean;
  employeePrice: number;
  externalPrice: number;
  allergens: string[];
}

export default function DailyKioskPage() {
  const { selectedLocation } = useLocation();
  const [menuItems, setMenuItems] = useState<MenuItem[]>([]);
  const [error, setError] = useState<Error | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const categories = ['Breakfast', 'Lunch', 'AfternoonTea'] as const;

  useEffect(() => {
    if (!selectedLocation) return;

    const fetchMenu = async () => {
      try {
        const data = await getDailyMenu(selectedLocation.id);
        setMenuItems(data);
      } catch (err) {
        setError(err instanceof Error ? err : new Error('Failed to fetch menu'));
      } finally {
        setIsLoading(false);
      }
    };

    fetchMenu();

    // Set up periodic refresh
    const refreshInterval = setInterval(fetchMenu, 60000); // Refresh every minute

    return () => clearInterval(refreshInterval);
  }, [selectedLocation]);

  if (!selectedLocation) {
    return (
      <div className="min-h-screen bg-gray-100 flex items-center justify-center">
        <div className="text-2xl text-gray-600">Please select a location to view the menu.</div>
      </div>
    );
  }

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-100 flex items-center justify-center">
        <div className="text-2xl text-gray-600">Loading menu...</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen bg-gray-100 flex items-center justify-center">
        <div className="text-2xl text-red-600">Error: {error.message}</div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-100">
      <div className="container mx-auto px-4 py-8">
        <h1 className="text-5xl font-bold text-center mb-12">Today&apos;s Menu</h1>

        <div className="space-y-12">
          {categories.map((category) => {
            const items = menuItems.filter((item) => item.type === category);
            if (items.length === 0) return null;

            return (
              <section key={category} className="bg-white rounded-2xl shadow-lg p-8">
                <h2 className="text-3xl font-semibold mb-6">{category}</h2>
                <div className="grid gap-8 md:grid-cols-2 lg:grid-cols-3">
                  {items.map((item) => (
                    <div
                      key={item.id}
                      className="bg-gray-50 rounded-xl p-6 hover:shadow-md transition-shadow"
                    >
                      <h3 className="text-2xl font-semibold mb-2 flex items-center gap-2">
                        {item.name}
                        {item.isSpecialOffer && (
                          <span className="bg-green-100 text-green-800 text-sm px-3 py-1 rounded-full">
                            Special
                          </span>
                        )}
                      </h3>
                      <p className="text-gray-600 text-lg mb-4">{item.description}</p>
                      <div className="flex justify-between items-end">
                        <div>
                          <p className="text-sm text-gray-500">Employee Price</p>
                          <p className="text-2xl font-bold text-green-600">
                            €{item.employeePrice.toFixed(2)}
                          </p>
                        </div>
                        <div className="text-right">
                          <p className="text-sm text-gray-500">External Price</p>
                          <p className="text-2xl font-bold text-blue-600">
                            €{item.externalPrice.toFixed(2)}
                          </p>
                        </div>
                      </div>
                      {item.allergens.length > 0 && (
                        <div className="mt-4 flex flex-wrap gap-1">
                          {item.allergens.map((allergen) => (
                            <span
                              key={allergen}
                              className="bg-yellow-100 text-yellow-800 px-2 py-1 rounded text-sm"
                            >
                              {allergen}
                            </span>
                          ))}
                        </div>
                      )}
                    </div>
                  ))}
                </div>
              </section>
            );
          })}
        </div>
      </div>
    </div>
  );
}