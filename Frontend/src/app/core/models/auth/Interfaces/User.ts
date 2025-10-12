export interface User {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  profileImagePath?: string;
  createdAt?: Date;
}