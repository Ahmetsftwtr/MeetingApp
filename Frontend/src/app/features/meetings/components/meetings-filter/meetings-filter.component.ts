import { Component, output, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IconComponent } from '../../../../shared/components/icon/icon.component';

export interface FilterValues {
  searchTerm: string;
  startDateFrom: string;
  startDateTo: string;
  endDateFrom: string;
  endDateTo: string;
  sortBy: string;
  sortDesc: boolean;
  pageSize: number;
}

@Component({
  selector: 'app-meetings-filter',
  imports: [CommonModule, FormsModule, IconComponent],
  templateUrl: './meetings-filter.component.html',
  styleUrl: './meetings-filter.component.scss'
})
export class MeetingsFilterComponent {
  filterChange = output<FilterValues>();
  clearFilters = output<void>();
  
  showFilters = signal(false);
  searchTerm = signal('');
  startDateFrom = '';
  startDateTo = '';
  endDateFrom = '';
  endDateTo = '';
  sortBy = signal('StartDate');
  sortDesc = signal(true);
  pageSize = signal(10);

  hasActiveFilters = computed(() => 
    this.searchTerm() !== '' || 
    this.startDateFrom !== '' || 
    this.startDateTo !== '' || 
    this.endDateFrom !== '' || 
    this.endDateTo !== ''
  );

  toggleFilters() {
    this.showFilters.set(!this.showFilters());
  }

  onSearchChange(value: string) {
    this.searchTerm.set(value);
    this.emitFilterChange();
  }

  applyFilters() {
    this.emitFilterChange();
  }

  onSortChange(field: string) {
    if (this.sortBy() === field) {
      this.sortDesc.set(!this.sortDesc());
    } else {
      this.sortBy.set(field);
      this.sortDesc.set(true);
    }
    this.emitFilterChange();
  }

  onPageSizeChange(value: string) {
    this.pageSize.set(Number(value));
    this.emitFilterChange();
  }

  onClearFilters() {
    this.searchTerm.set('');
    this.startDateFrom = '';
    this.startDateTo = '';
    this.endDateFrom = '';
    this.endDateTo = '';
    this.sortBy.set('StartDate');
    this.sortDesc.set(true);
    this.pageSize.set(10);
    this.clearFilters.emit();
  }

  private emitFilterChange() {
    this.filterChange.emit({
      searchTerm: this.searchTerm(),
      startDateFrom: this.startDateFrom,
      startDateTo: this.startDateTo,
      endDateFrom: this.endDateFrom,
      endDateTo: this.endDateTo,
      sortBy: this.sortBy(),
      sortDesc: this.sortDesc(),
      pageSize: this.pageSize()
    });
  }
}
