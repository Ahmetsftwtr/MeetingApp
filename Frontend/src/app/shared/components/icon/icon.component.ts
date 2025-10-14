import { Component, input } from '@angular/core';

export type IconName = 
  | 'calendar'
  | 'clock'
  | 'document'
  | 'download'
  | 'trash'
  | 'edit'
  | 'plus'
  | 'arrow-left'
  | 'arrow-right'
  | 'arrow-up'
  | 'arrow-down'
  | 'check'
  | 'close'
  | 'user'
  | 'users'
  | 'search'
  | 'filter'
  | 'upload'
  | 'logout'
  | 'settings'
  | 'info'
  | 'warning'
  | 'error'
  | 'success';

@Component({
  selector: 'app-icon',
  imports: [],
  templateUrl: './icon.component.html',
  styleUrl: './icon.component.scss'
})
export class IconComponent {
  name = input.required<IconName>();
  size = input<number>(24);
  color = input<string>('currentColor');
}
