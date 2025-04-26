import { useQuery } from '@tanstack/react-query';
import { Tab } from '@headlessui/react';
import { Fragment } from 'react';
import MenuItem from './MenuItem';

interface MenuItemType {
  id: string;
  name: string;
  description: string;
  employeePrice: number;
  externalPrice: number;
  allergens: string[];
  type: 'Breakfast' | 'Lunch' | 'AfternoonTea';
  isSpecialOffer: boolean;
  specialOfferDay: number | null;
}

export default function DailyMenu() {
  const { data: menuItems, isLoading } = useQuery<MenuItemType[]>({
    queryKey: ['dailyMenu'],
    queryFn: async () => {
      const response = await fetch('/api/menu/today');
      if (!response.ok) throw new Error('Failed to fetch menu');
      return response.json();
    },
  });

  if (isLoading) {
    return <div>Loading menu...</div>;
  }

  const categories = ['Breakfast', 'Lunch', 'AfternoonTea'];
  const categorizedItems = categories.map(category => ({
    name: category,
    items: menuItems?.filter(item => item.type === category) || [],
  }));

  return (
    <div className="w-full max-w-4xl mx-auto">
      <Tab.Group>
        <Tab.List className="flex space-x-1 rounded-xl bg-blue-900/20 p-1">
          {categories.map((category) => (
            <Tab
              key={category}
              className={({ selected }) =>
                `w-full rounded-lg py-2.5 text-sm font-medium leading-5
                 ${selected
                  ? 'bg-white shadow text-blue-700'
                  : 'text-blue-100 hover:bg-white/[0.12] hover:text-white'
                }`
              }
            >
              {category}
            </Tab>
          ))}
        </Tab.List>
        <Tab.Panels className="mt-2">
          {categorizedItems.map((category, idx) => (
            <Tab.Panel
              key={idx}
              className="rounded-xl bg-white p-3 ring-white/60 ring-offset-2 ring-offset-blue-400 focus:outline-none focus:ring-2"
            >
              <div className="grid gap-4">
                {category.items.map((item) => (
                  <MenuItem key={item.id} item={item} />
                ))}
                {category.items.length === 0 && (
                  <p className="text-gray-500">No items available for this category.</p>
                )}
              </div>
            </Tab.Panel>
          ))}
        </Tab.Panels>
      </Tab.Group>
    </div>
  );
}