export type MenuType = 'Breakfast' | 'Lunch' | 'AfternoonTea';

export interface MenuItem {
  id: string;
  name: string;
  description: string;
  type: MenuType;
  employeePrice: number;
  externalPrice: number;
  isSpecialOffer: boolean;
  allergens: string[];
}