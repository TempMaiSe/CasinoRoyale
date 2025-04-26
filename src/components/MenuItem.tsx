interface MenuItemProps {
  item: {
    id: string;
    name: string;
    description: string;
    employeePrice: number;
    externalPrice: number;
    allergens: string[];
    isSpecialOffer: boolean;
  };
}

export default function MenuItem({ item }: MenuItemProps) {
  return (
    <div className="p-4 border rounded-lg shadow-sm hover:shadow-md transition-shadow">
      <div className="flex justify-between items-start">
        <div>
          <h3 className="text-lg font-semibold">
            {item.name}
            {item.isSpecialOffer && (
              <span className="ml-2 inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-green-100 text-green-800">
                Special Offer
              </span>
            )}
          </h3>
          <p className="text-gray-600 mt-1">{item.description}</p>
          {item.allergens.length > 0 && (
            <div className="mt-2">
              <span className="text-sm text-gray-500">Allergens: </span>
              {item.allergens.map((allergen, index) => (
                <span
                  key={allergen}
                  className="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-yellow-100 text-yellow-800 mr-1"
                >
                  {allergen}
                </span>
              ))}
            </div>
          )}
        </div>
        <div className="text-right">
          <div className="text-lg font-bold text-green-600">€{item.employeePrice.toFixed(2)}</div>
          <div className="text-sm text-gray-500">External: €{item.externalPrice.toFixed(2)}</div>
        </div>
      </div>
    </div>
  );
}