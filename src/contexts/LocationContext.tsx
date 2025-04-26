'use client';

import { createContext, useContext, useState, useEffect, useMemo } from 'react';

interface Location {
  id: string;
  name: string;
  timeZoneId: string;
  isActive: boolean;
}

interface LocationContextType {
  locations: Location[];
  selectedLocation?: Location;
  setSelectedLocation: (location: Location) => void;
  isLoading: boolean;
  error?: string;
}

const LocationContext = createContext<LocationContextType>({
  locations: [],
  setSelectedLocation: () => {},
  isLoading: true
});

type LocationProviderProps = Readonly<{
  children: React.ReactNode;
}>;

export function LocationProvider({ children }: LocationProviderProps) {
  const [locations, setLocations] = useState<Location[]>([]);
  const [selectedLocation, setSelectedLocation] = useState<Location>();
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string>();

  useEffect(() => {
    fetch('/api/locations')
      .then(response => {
        if (!response.ok) throw new Error('Failed to fetch locations');
        return response.json();
      })
      .then(data => {
        setLocations(data);
        // Set first active location as default if none selected
        if (!selectedLocation && data.length > 0) {
          const defaultLocation = data.find((l: Location) => l.isActive);
          if (defaultLocation) setSelectedLocation(defaultLocation);
        }
      })
      .catch(err => setError(err.message))
      .finally(() => setIsLoading(false));
  }, [selectedLocation]);

  const value = useMemo(() => ({
    locations,
    selectedLocation,
    setSelectedLocation,
    isLoading,
    error
  }), [locations, selectedLocation, isLoading, error]);

  return (
    <LocationContext.Provider value={value}>
      {children}
    </LocationContext.Provider>
  );
}

export function useLocation() {
  return useContext(LocationContext);
}