import { DateTimeInfoDto } from '../../../../FooReflection/Domain/Dto/Common/DateTimeInfoDto';
import { RoleType } from '../../../../FooReflection/Domain/Enum/Common/RoleType';

export interface RoleDto {
  id: number;
  type: RoleType;
  name: string;
  dateTimeInfo: DateTimeInfoDto;
}
