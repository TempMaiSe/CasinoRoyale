import { clientEnv } from '@/lib/env';
import { notFound } from 'next/navigation';

interface DishPageProps {
  params: {
    id: string;
  };
}

async function getDish(id: string) {
  const res = await fetch(`${clientEnv.apiUrl}/api/menu/dish/${id}`, {
    next: { revalidate: 60 }, // Revalidate every minute
  });

  if (!res.ok) return null;
  return res.json();
}

export default async function DishPage({ params }: DishPageProps) {
  const dish = await getDish(params.id);

  if (!dish) {
    notFound();
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-100 p-4">
      <div className="bg-white rounded-2xl shadow-xl p-8 max-w-4xl w-full">
        <h1 className="text-4xl font-bold mb-4">{dish.name}</h1>
        <p className="text-xl text-gray-600 mb-6">{dish.description}</p>
        
        <div className="grid grid-cols-2 gap-8 mb-8">
          <div className="text-center p-6 bg-green-50 rounded-xl">
            <div className="text-sm text-gray-600 mb-2">Employee Price</div>
            <div className="text-5xl font-bold text-green-600">€{dish.employeePrice.toFixed(2)}</div>
          </div>
          <div className="text-center p-6 bg-blue-50 rounded-xl">
            <div className="text-sm text-gray-600 mb-2">External Price</div>
            <div className="text-5xl font-bold text-blue-600">€{dish.externalPrice.toFixed(2)}</div>
          </div>
        </div>

        {dish.allergens.length > 0 && (
          <div className="mt-6">
            <h2 className="text-xl font-semibold mb-3">Allergens</h2>
            <div className="flex flex-wrap gap-2">
              {dish.allergens.map((allergen: string) => (
                <span
                  key={allergen}
                  className="px-4 py-2 bg-yellow-100 text-yellow-800 rounded-full text-lg"
                >
                  {allergen}
                </span>
              ))}
            </div>
          </div>
        )}

        {dish.isSpecialOffer && (
          <div className="mt-8 bg-green-100 p-4 rounded-xl">
            <span className="text-xl font-semibold text-green-800">
              Special Offer!
            </span>
          </div>
        )}
      </div>
    </div>
  );
}