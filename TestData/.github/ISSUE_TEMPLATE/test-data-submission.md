---
name: Test Data Submission
about: Submit new test or example data to the collection
title: '[TEST DATA] '
labels: test-data, community
assignees: ''
---

## Test Data Submission

### Basic Information

**Test Data ID:** (e.g., my-test-case-001)
**Name:** (Brief descriptive name)
**Category:** (link-operations | sequences | transformations | indexing | unicode | import-export | performance | other)
**Difficulty:** (basic | intermediate | advanced)

### Description

Provide a clear and detailed description of what this test data demonstrates:

### Input Data

```json
{
  "description": "Description of input",
  "data": {
    // Your input data here
  }
}
```

### Expected Output

```json
{
  "description": "Description of expected output",
  "data": {
    // Your expected output data here
  }
}
```

### Rules Applied

List the rules or algorithms that should be applied to transform the input to the expected output:

1. Rule 1
2. Rule 2
3. ...

### Use Case

Describe the real-world scenario or problem this test data represents:

### Additional Notes

Any additional context, edge cases, or considerations:

### Verification

- [ ] I have verified that this test data is accurate
- [ ] I have followed the test data format specification
- [ ] I have ensured no sensitive or proprietary information is included
- [ ] I have tagged the submission appropriately
- [ ] I have provided complete input and expected output data

### Author Information

**Author Name:**
**Date:**
**Contact (optional):**

---

**For Reviewers:**

- [ ] Format validation passed
- [ ] Data is accurate and complete
- [ ] Description is clear and helpful
- [ ] No sensitive information included
- [ ] Appropriate category and tags
- [ ] Ready to merge to community or standard collection
