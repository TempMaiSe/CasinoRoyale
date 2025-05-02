'use client';

import { clientEnv } from '@/lib/env';
import { MenuType } from '@/types/menu';
import { useEffect, useState } from 'react';

async function getDailyMenu() {
  const res = await fetch(`${clientEnv.apiUrl}/api/menu/today`, {
    next: { revalidate: 60 }, // Revalidate every minute
  });

  if (!res.ok) throw new Error('Failed to fetch menu');
  return res.json();
}

export default function DailyKioskPage() {
  const [menuItems, setMenuItems] = useState<any[]>([]);
  const [error, setError] = useState<Error | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const categories = ['Breakfast', 'Lunch', 'AfternoonTea'] as const;

  useEffect(() => {
    const fetchMenu = async () => {
      try {
        const data = await getDailyMenu();
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
  }, []);

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
        <h1 className="text-5xl font-bold text-center mb-12">Today's Menu</h1>

        <div className="space-y-12">
          {categories.map((category) => {
            const items = menuItems.filter((item: any) => item.type === category);
            if (items.length === 0) return null;

            return (
              <section key={category} className="bg-white rounded-2xl shadow-lg p-8">
                <h2 className="text-3xl font-semibold mb-6">{category}</h2>
                <div className="grid gap-8 md:grid-cols-2 lg:grid-cols-3">
                  {items.map((item: any) => (
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
                          {item.allergens.map((allergen: string) => (
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