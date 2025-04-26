import DailyMenu from '@/components/DailyMenu';

export default function Home() {
  return (
    <main className="min-h-screen p-4">
      <h1 className="text-2xl font-bold mb-4">Today's Menu</h1>
      <DailyMenu />
    </main>
  );
}
