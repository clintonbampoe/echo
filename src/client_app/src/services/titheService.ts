import type { TitheRecord, TitheSummary } from '../types/tithe';

// Mock Data
const mockTithes: TitheRecord[] = [
  {
    titheId: 1,
    uniqueId: 'tithe-001',
    memberId: 101,
    amount: 1500,
    paymentMethod: 'MobileMoney',
    collectionDate: '2026-05-02',
    forMonth: 'May',
    forYear: 2026,
    description: null,
    memberName: 'John Doe',
  },
  {
    titheId: 2,
    uniqueId: 'tithe-002',
    memberId: 102,
    amount: 2000,
    paymentMethod: 'Cash',
    collectionDate: '2026-05-09',
    forMonth: 'May',
    forYear: 2026,
    description: 'May tithe',
    memberName: 'Jane Smith',
  },
  {
    titheId: 3,
    uniqueId: 'tithe-003',
    memberId: 103,
    amount: 1000,
    paymentMethod: 'CreditCard',
    collectionDate: '2026-05-15',
    forMonth: 'May',
    forYear: 2026,
    description: null,
    memberName: 'Baba Tundey',
  },
  {
    titheId: 4,
    uniqueId: 'tithe-004',
    memberId: 101,
    amount: 1500,
    paymentMethod: 'MobileMoney',
    collectionDate: '2026-04-02',
    forMonth: 'April',
    forYear: 2026,
    description: null,
    memberName: 'John Doe',
  }
];

export const getTitheRecords = async (month?: string, year?: number): Promise<TitheRecord[]> => {
  return new Promise((resolve) => {
    setTimeout(() => {
      let filtered = mockTithes;
      if (month) {
        filtered = filtered.filter(t => t.forMonth === month);
      }
      if (year) {
        filtered = filtered.filter(t => t.forYear === year);
      }
      // Return sorted by date descending
      filtered.sort((a, b) => new Date(b.collectionDate).getTime() - new Date(a.collectionDate).getTime());
      resolve(filtered);
    }, 300);
  });
};

export const getTitheSummary = async (month?: string, year?: number): Promise<TitheSummary> => {
  return new Promise((resolve) => {
    setTimeout(() => {
      let filtered = mockTithes;
      if (month) {
        filtered = filtered.filter(t => t.forMonth === month);
      }
      if (year) {
        filtered = filtered.filter(t => t.forYear === year);
      }
      
      const totalAmount = filtered.reduce((sum, t) => sum + t.amount, 0);
      
      resolve({
        totalAmount,
        transactionCount: filtered.length,
        lastUpdated: new Date().toISOString()
      });
    }, 200);
  });
};

export const deleteTitheRecord = async (titheId: number): Promise<boolean> => {
  return new Promise((resolve) => {
    setTimeout(() => {
      const index = mockTithes.findIndex(t => t.titheId === titheId);
      if (index > -1) {
        mockTithes.splice(index, 1);
      }
      resolve(true);
    }, 300);
  });
};

export const saveTitheRecord = async (
  record: Omit<TitheRecord, 'titheId' | 'uniqueId' | 'memberName'> & { titheId?: number; memberName?: string }
): Promise<TitheRecord> => {
  return new Promise((resolve) => {
    setTimeout(() => {
      if (record.titheId) {
        const index = mockTithes.findIndex(t => t.titheId === record.titheId);
        if (index > -1) {
          const updated = { ...mockTithes[index], ...record } as TitheRecord;
          mockTithes[index] = updated;
          resolve(updated);
          return;
        }
      }
      const nextId = mockTithes.length > 0 ? Math.max(...mockTithes.map(t => t.titheId)) + 1 : 1;
      const newRecord: TitheRecord = {
        titheId: nextId,
        uniqueId: `tithe-${String(nextId).padStart(3, '0')}`,
        memberId: record.memberId,
        amount: record.amount,
        paymentMethod: record.paymentMethod,
        collectionDate: record.collectionDate,
        forMonth: record.forMonth,
        forYear: record.forYear,
        description: record.description || null,
        memberName: record.memberName,
      };
      mockTithes.push(newRecord);
      resolve(newRecord);
    }, 300);
  });
};
