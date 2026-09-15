import re
import os

file_path = 'c:/Users/Ebenezer Quayson/Desktop/echo/frontend/src/components/Members.tsx'
with open(file_path, 'r', encoding='utf-8') as f:
    content = f.read()

# Fix GHANA_REGIONS
regions_repl = '''const GHANA_REGIONS = [
  { value: 'GreaterAccra', label: 'Greater Accra Region' },
  { value: 'Ashanti', label: 'Ashanti Region' },
  { value: 'Eastern', label: 'Eastern Region' },
  { value: 'Western', label: 'Western Region' },
  { value: 'Central', label: 'Central Region' },
  { value: 'Northern', label: 'Northern Region' },
  { value: 'UpperEast', label: 'Upper East Region' },
  { value: 'UpperWest', label: 'Upper West Region' },
  { value: 'Volta', label: 'Volta Region' },
  { value: 'Bono', label: 'Bono Region' },
  { value: 'Oti', label: 'Oti Region' },
  { value: 'Savannah', label: 'Savannah Region' },
  { value: 'NorthEast', label: 'North East Region' },
  { value: 'BonoEast', label: 'Bono East Region' },
  { value: 'Ahafo', label: 'Ahafo Region' },
  { value: 'WesternNorth', label: 'Western North Region' },
];'''
content = re.sub(r'const GHANA_REGIONS = \[.*?\];', regions_repl, content, flags=re.DOTALL)

# Fix map over GHANA_REGIONS
content = re.sub(r'\{GHANA_REGIONS\.map\(r => <option key=\{r\} value=\{r\}>\{r\}</option>\)\}', '{GHANA_REGIONS.map(r => <option key={r.value} value={r.value}>{r.label}</option>)}', content)

# Fix NewVisitor in Tabs
content = re.sub(r"const tabs = \['All Members', 'Active', 'New Visitors', 'Archived'\];", "const tabs = ['All Members', 'Active', 'Inactive', 'Archived'];", content)
content = re.sub(r"if \(activeTab === 'New Visitors'\) backendStatus = 'NewVisitor';", "", content)
content = re.sub(r"if \(activeTab === 'Archived'\) backendStatus = 'Inactive';", "if (activeTab === 'Inactive') backendStatus = 'Inactive';\n  if (activeTab === 'Archived') backendStatus = 'Archived';", content)
content = re.sub(r"const newMembers = members\.filter\(m => m\.status === 'NewVisitor'\)\.length;", "const newMembers = members.filter(m => m.status === 'Archived').length;", content)

# Fix Select Options
content = re.sub(r'<option value="Divorced">Divorced</option>', '', content)
content = re.sub(r'<option value="NewVisitor">New Visitor</option>', '<option value="Archived">Archived</option>', content)

# Fix form emptyForm
content = re.sub(r"status: 'Active',", "status: 'Active',", content)

# Save Members.tsx
with open(file_path, 'w', encoding='utf-8') as f:
    f.write(content)

# Fix member type definitions in member.ts
member_ts_path = 'c:/Users/Ebenezer Quayson/Desktop/echo/frontend/src/types/member.ts'
with open(member_ts_path, 'r', encoding='utf-8') as f:
    m_content = f.read()

m_content = m_content.replace("'Single' | 'Married' | 'Divorced' | 'Widowed'", "'Single' | 'Married' | 'Widowed'")
m_content = m_content.replace("'Active' | 'Inactive' | 'NewVisitor'", "'Active' | 'Inactive' | 'Archived' | 'Transferred'")

with open(member_ts_path, 'w', encoding='utf-8') as f:
    f.write(m_content)

print("Done fixing enums")
