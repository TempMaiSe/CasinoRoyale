'use client';

import { useAuth } from '@/contexts/AuthContext';
import { useRouter } from 'next/navigation';
import { useEffect, useState } from 'react';
import { clientEnv } from '@/lib/env';
import LocationSelector from '@/components/LocationSelector';
import { useLocation } from '@/contexts/LocationContext';

interface MenuItemFormData {
  name: string;
  description: string;
  employeePrice: number;
  externalPrice: number;
  allergens: string[];
  type: 'Breakfast' | 'Lunch' | 'AfternoonTea';
  isSpecialOffer: boolean;
  specialOfferDay: number | null;
}

export default function AdminPage() {
  const { isAuthenticated, isAdmin, token, login } = useAuth();
  const { selectedLocation } = useLocation();
  const router = useRouter();
  const [formData, setFormData] = useState<MenuItemFormData>({
    name: '',
    description: '',
    employeePrice: 0,
    externalPrice: 0,
    allergens: [],
    type: 'Lunch',
    isSpecialOffer: false,
    specialOfferDay: null,
  });

  useEffect(() => {
    if (!isAuthenticated) {
      login();
    } else if (!isAdmin) {
      router.push('/');
    }
  }, [isAuthenticated, isAdmin, login, router]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!selectedLocation) {
      alert('Please select a location before adding a menu item.');
      return;
    }

    try {
      const response = await fetch(`${clientEnv.apiUrl}/api/locations/${selectedLocation.id}/menu/items`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`,
        },
        body: JSON.stringify({ ...formData, locationId: selectedLocation.id }),
      });

      if (response.ok) {
        setFormData({
          name: '',
          description: '',
          employeePrice: 0,
          externalPrice: 0,
          allergens: [],
          type: 'Lunch',
          isSpecialOffer: false,
          specialOfferDay: null,
        });
        alert('Menu item added successfully!');
      } else {
        throw new Error('Failed to add menu item');
      }
    } catch (error) {
      console.error('Error adding menu item:', error);
      alert('Failed to add menu item. Please try again.');
    }
  };

  if (!isAuthenticated || !isAdmin) {
    return <div>Loading...</div>;
  }

  return (
    <div className="min-h-screen bg-gray-100 py-8">
      <div className="max-w-4xl mx-auto px-4">
        <h1 className="text-3xl font-bold mb-8">Menu Management</h1>

        <LocationSelector />

        <form onSubmit={handleSubmit} className="bg-white rounded-lg shadow-md p-6">
          <div className="grid grid-cols-1 gap-6">
            <div>
              <label className="block text-sm font-medium text-gray-700">Name</label>
              <input
                type="text"
                required
                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700">Description</label>
              <textarea
                required
                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm"
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              />
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700">Employee Price (€)</label>
                <input
                  type="number"
                  step="0.01"
                  required
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm"
                  value={formData.employeePrice}
                  onChange={(e) => setFormData({ ...formData, employeePrice: parseFloat(e.target.value) })}
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700">External Price (€)</label>
                <input
                  type="number"
                  step="0.01"
                  required
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm"
                  value={formData.externalPrice}
                  onChange={(e) => setFormData({ ...formData, externalPrice: parseFloat(e.target.value) })}
                />
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700">Type</label>
              <select
                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm"
                value={formData.type}
                onChange={(e) => setFormData({ ...formData, type: e.target.value as any })}
              >
                <option value="Breakfast">Breakfast</option>
                <option value="Lunch">Lunch</option>
                <option value="AfternoonTea">Afternoon Tea</option>
              </select>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700">Allergens</label>
              <input
                type="text"
                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm"
                placeholder="Comma-separated list of allergens"
                value={formData.allergens.join(', ')}
                onChange={(e) => setFormData({
                  ...formData,
                  allergens: e.target.value.split(',').map(a => a.trim()).filter(Boolean)
                })}
              />
            </div>

            <div className="flex items-center">
              <input
                type="checkbox"
                className="rounded border-gray-300 text-blue-600 shadow-sm"
                checked={formData.isSpecialOffer}
                onChange={(e) => setFormData({ ...formData, isSpecialOffer: e.target.checked })}
              />
              <label className="ml-2 text-sm text-gray-700">Special Offer</label>
            </div>

            {formData.isSpecialOffer && (
              <div>
                <label className="block text-sm font-medium text-gray-700">Special Offer Day</label>
                <select
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm"
                  value={formData.specialOfferDay ?? ''}
                  onChange={(e) => setFormData({
                    ...formData,
                    specialOfferDay: e.target.value ? parseInt(e.target.value) : null
                  })}
                >
                  <option value="">Select a day</option>
                  <option value="1">Monday</option>
                  <option value="2">Tuesday</option>
                  <option value="3">Wednesday</option>
                  <option value="4">Thursday</option>
                  <option value="5">Friday</option>
                </select>
              </div>
            )}

            <button
              type="submit"
              className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700"
            >
              Add Menu Item
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}