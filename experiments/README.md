# Experiments Directory

This directory contains experimental implementations and proof-of-concept code for exploring new features and technologies in the Links Platform.

## Current Experiments

### StorJ Distributed Transaction Log Storage

**Status**: Proof of Concept
**Issue**: [#594](https://github.com/konard/LinksPlatform/issues/594)
**Files**:
- `StorjTransactionLog.cs` - Core implementation of distributed transaction log using StorJ
- `StorjTransactionLogExample.cs` - Usage examples and demonstration code
- `STORJ_INTEGRATION.md` - Comprehensive documentation

**Description**:
This experiment implements distributed transaction log storage using StorJ, a decentralized cloud storage platform. The implementation provides:
- S3-compatible interface for transaction log storage
- Asynchronous transaction append and retrieval
- Hierarchical organization for scalability
- Integration strategy with existing local transaction logs

**Usage**:
See `STORJ_INTEGRATION.md` for detailed setup and usage instructions.

## Purpose of This Directory

The `experiments/` directory serves as a sandbox for:

1. **Prototyping**: Testing new ideas and approaches before full integration
2. **Side Projects**: Implementing features marked as "Side Project" in issues
3. **Research**: Exploring new technologies and integration possibilities
4. **Learning**: Providing examples for understanding complex concepts

## Guidelines

### Code in Experiments

- Code here is **not production-ready** by default
- May lack comprehensive error handling
- May not follow all production coding standards
- Serves as a proof-of-concept or learning material

### Moving to Production

When an experiment is ready for production:

1. Refactor code to meet production standards
2. Add comprehensive error handling
3. Write unit and integration tests
4. Create proper project structure in `Platform/` directory
5. Update documentation
6. Submit for full code review

## Related Directories

- `/Platform/` - Production code
- `/Platform/Platform.Sandbox/` - Sandbox for testing platform features
- `/Platform/Platform.Examples/` - Production-ready example code
- `/doc/` - Official documentation

## Contributing

Feel free to add new experiments to this directory:

1. Create a new subdirectory or file(s) for your experiment
2. Include a clear description in this README
3. Add documentation explaining the purpose and usage
4. Reference related issues if applicable
5. Mark the status clearly (Proof of Concept, Work in Progress, Ready for Production)

## Notes

- Experiments may be incomplete or in various stages of development
- Not all experiments will make it to production
- Feedback and improvements are welcome via pull requests
- For questions about specific experiments, refer to the related issue or documentation
