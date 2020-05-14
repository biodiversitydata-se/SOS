# Data Transfer Objects (DTOs)

There should be a translation between the domain model and the data contracts. That way you will be able to keep evolving your domain model and at the same time preserve the data contracts for backward compatibility. So anytime you refactor something in the model, you only modify the mappings, but keep the contracts themselves unchanged. 
>DTOs can be used for translation between the domain model and the data contract.

![Translation between the domain model and the data contracts](../docs/images/dto_data_contract_external_world.png)
</br>
</br>
### Problems with serializing domain entities
If you serialize the domain entities, then you couple the the domain model to data contracts.
>It means that you will not be capable of doing any refactoring in your domain model whatsoever, any such refactoring would lead to violating the contract between you and your clients.
Use DTOs to decouple the domain model from the data contracts.

![Serialization of domain entities](../docs/images/serialization_of_domain_entities_coupling_refactoring.png)

Source: _[Refactoring from Anemic Domain Model Towards a Rich One, by Vladimir Khorikov](https://app.pluralsight.com/library/courses/refactoring-anemic-domain-model/)_