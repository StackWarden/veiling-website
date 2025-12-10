export type Product = {
  id: string;
  supplierId: string;
  species: string;
  potSize: string;
  stemLength: number;
  quantity: number;
  minPrice: number;
  clockLocation: string | number;
  auctionDate?: string | null;
  photoUrl?: string | null;
};