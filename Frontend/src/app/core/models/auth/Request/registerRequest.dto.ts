export interface RegisterRequestDto {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  password: string;
  profileImage?: File;
}