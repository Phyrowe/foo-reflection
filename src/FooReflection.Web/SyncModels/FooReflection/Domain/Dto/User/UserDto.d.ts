import { DateTimeInfoDto } from '../../../../FooReflection/Domain/Dto/Common/DateTimeInfoDto';
import { RoleDto } from '../../../../FooReflection/Domain/Dto/Role/RoleDto';

export interface UserDto {
  id: number;
  username: string;
  role: RoleDto;
  dateTimeInfo: DateTimeInfoDto;
}
