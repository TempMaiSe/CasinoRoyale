import { useEffect, useState } from 'react';
import MenuItem from '@/components/MenuItem';
import { useLocation } from '@/contexts/LocationContext';

interface MenuItem {
  id: string;
  name: string;
  description: string;
  employeePrice: number;
  externalPrice: number;
  allergens: string[];
  type: string;
  isSpecialOffer: boolean;
  specialOfferDay?: number;
}

export default function DailyMenu() {
  const [menuItems, setMenuItems] = useState<MenuItem[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string>();
  const { selectedLocation } = useLocation();

  useEffect(() => {
    if (!selectedLocation) {
      setMenuItems([]);
      setIsLoading(false);
      return;
    }

    setIsLoading(true);
    fetch(`/api/locations/${selectedLocation.id}/menu/today`)
      .then(response => {
        if (!response.ok) throw new Error('Failed to fetch menu');
        return response.json();
      })
      .then(data => setMenuItems(data))
      .catch(err => setError(err.message))
      .finally(() => setIsLoading(false));
  }, [selectedLocation]);

  if (!selectedLocation) {
    return <div>Please select a location to view the menu</div>;
  }

  if (isLoading) return <div>Loading menu...</div>;
  if (error) return <div className="text-red-600">Error: {error}</div>;
  if (!menuItems.length) return <div>No menu items available</div>;

  const groupedItems = menuItems.reduce((acc, item) => {
    const type = item.type;
    if (!acc[type]) acc[type] = [];
    acc[type].push(item);
    return acc;
  }, {} as Record<string, MenuItem[]>);

  return (
    <div className="space-y-8">
      {Object.entries(groupedItems).map(([type, items]) => (
        <div key={type}>
          <h2 className="text-2xl font-bold mb-4 text-gray-900">{type}</h2>
          <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
            {items.map(item => (
              <MenuItem key={item.id} item={item} />
            ))}
          </div>
        </div>
      ))}
    </div>
  );
}