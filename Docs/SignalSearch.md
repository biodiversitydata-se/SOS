# Signal Search – Documentation

## 1. Overview

Signal Search is a feature that enables searching for **sensitive observations** without revealing sensitive information. The result of a signal search is a binary response: **Yes** or **No**, indicating whether there is at least one sensitive observation that matches the specified search criteria.

Signal Search is designed to:

* Protect species and locations by not exposing sighting details.
* Enable authorities and other authorized actors to make decisions based on occurrence information. A *Yes* response can serve as an initial indication for an employee authorised to perform signal searches, who may then contact a person with higher authorization that has access to more detailed information about the observation(s) and can decide on further actions.
* Ensure that only authorized users can perform searches, and only within permitted geographic areas.

> **Important:** Only sensitive observations are included in Signal Search. Public observations are never included.

What are **sensitive observations**? Those are species observation records that have been judged to contain sensitive information which if released to the public, would result in an adverse effect on the taxon or attribute in question or to a living individual. Sensitive information commonly relates to the exact localities of rare, endangered or commercially valuable taxa. Therefore, access to observations classified as sensitive are provided with restricted access only. Some sensitive observations are also subject to confidentiality under the Swedish Public Access to Information and Secrecy Act (2009:400), Chapter 20, Section 1.

Further information: [Sensitivity of species observation data (SLU Artdatabanken)](https://www.slu.se/artdatabanken/rapportering-och-fynd/fynduppgifter-och-skyddsklassade-arter/skyddsklassade-arter) [Skyddsklassning]

---

## 2. What does a Yes/No response mean?

* **Yes (true):** There is at least one restricted access observation that matches the search criteria.
* **No (false):** There are no restricted access observations that match the search criteria.

The user is **not** informed about:

* Which species were matched
* The number of observations
* Exact or approximate locations

---

## 3. Authorization and access

### 3.1 Basic authorization requirements

To perform a signal search, the user must have:

* A role that grants access to **Signal Search**. The role must include a permission with the identifier `SightingIndication`.

Authorization can be granted:

* For all of Sweden (uncommon)
* For a restricted area, such as a county

### 3.2 Geographic authorization limitations

If a user attempts to perform a signal search in an area for which they lack authorization, the following may occur:

* **HTTP 403 (Forbidden):** The user lacks authorization for the entire search area.
* **HTTP 409 (Conflict):** The user has authorization for parts of the search area.

This behavior is controlled by the parameter `returnHttp4xxWhenNoPermissions`.

### 3.3 Signal Search and sensitivity categories

Signal Search searches among all sensitive observations, i.e. those with sensitivity categories 3, 4, and 5. It does not include observations with sensitivity category 1 (public observations). Sensitivity category 2 is an obsolete category and is no longer used.

When making a request to the endpoint, it is possible to specify that a signal should only be returned for observations with sensitivity categories above the user’s authorization level. For example, if a user has authorization up to sensitivity category 3, signals will then only be returned for categories 4 and 5.

It is not possible to restrict a signal search to specific sensitivity categories. The search is performed either across all sensitivity categories (3–5) or only those above the user’s authorization level.

It is not possible to restrict the search to a single taxon. At least one mandatory taxon list must be specified in the request. This design prevents identifying the exact species responsible for a signal.

---

## 4. Where is Signal Search available?

The Signal Search endpoint is available to internal applications at SLU and to external systems via the *Species Observation System API (Internal)* on the [Swedish Species Information Centre developer portal](https://api-portal.artdatabanken.se). Signal search is meant to be used primarily by authorities.

Signal Search is used in several applications, for example:

* **Artsök** (County Administrative Boards)
* **Navet** (Swedish Forest Agency)

There are two main types of users:

* **Personal users** (login via web interface)
* **Application users** (system-to-system)

---

## 5. Search criteria (Search Filter)

For a signal search to be valid, the following requirements must be met:

### 5.1 Geographic area (mandatory)

The search must target a geographic area:

* Region
* Polygon(s)
* Bounding box

A **region** is a predefined geographic area, such as a municipality or a county.

A **bounding box** is a rectangular area defined by its outer coordinate limits (minimum and maximum latitude and longitude).

A **polygon** is an area defined by a series of coordinates and may have any shape.

There is no minimum size restriction for the search area.

### 5.2 Taxon filter (mandatory)

* At least **one taxon list** must be provided.
* At least one of the taxon lists must be a **mandatory signal search taxon list**.

Signal Search taxon lists:

* Species protected by law (Id 1)
* Red listed species (Id 7)
* Habitats directive species (Id 8)
* Action plan species (Id 17)
* Swedish Forest Agency nature conservation species (Id 18)

If no mandatory taxon list is provided, **HTTP 400 (Bad Request)** is returned.

### 5.3 Date (mandatory)

* `StartDate` must be **at least one year in the past**.
* More recent dates result in a validation error.

### 5.4 Other filters (optional)

* Bird breeding status criterion (`BirdNestActivityLimit`)
* Dataset (`DataProvider`)
* Artportalen type filter (`ArtportalenTypeFilter`) – _Used to determine which types of observations in Artportalen should be included in the search. The default value is `DoNotShowMerged` and refers to the original reported records, not including any grouped observations based on posterior assessment. To include all underlying observations within a grouped observation and including replacements (corrected observations), `ShowChildrenAndReplacements` can be used._

---

## 6. Geographic filter

The geographic filter allows the user to specify how geographic information should be handled in signal searches, in order to account for positional uncertainty in observations and species sensitivity to disturbance.

### 6.1 How geographic information is stored

Each observation can be represented geographically in the following ways:

1. **Centroid (location.point)**.
   A point representing the reported position of the observation, together with a coordinate uncertainty (`coordinateUncertaintyInMeters`).

2. **Buffered geometry (location.pointWithBuffer)**.
For point-based observations, a circular polygon is created where the centroid is the observation point and the radius corresponds to the coordinate uncertainty (`coordinateUncertaintyInMeters`) i.e. the distance (in meters) from the given point describing the smallest circle containing the whole of the location.
For **polygon locations**, the **exact polygon** describing the true spatial extent of the observation is stored instead. Polygon locations are therefore not converted into circles.

3. **Disturbance area (location.pointWithDisturbanceBuffer)**.
A polygon created from the observation centroid and the taxon’s defined disturbance radius.
For polygon locations, the disturbance area is also based on the centroid combined with the disturbance radius.

### 6.2 How geometry is used in searches

Which geographic representation is used in a signal search is determined by which parameters are enabled. These parameters control whether the search should take into account positional uncertainty and/or species disturbance sensitivity.

* **considerObservationAccuracy = true**.
  The search is performed against the polygon area `location.pointWithBuffer`.
  Observations whose centroid lies outside the search geometry may still be included, provided that some part of the observation’s polygon area intersects or overlaps the search area.

* **considerDisturbanceRadius = true**.
  The search is performed against the disturbance area `location.pointWithDisturbanceBuffer`.
  Observations whose centroid lies outside the search geometry may still be included, provided that some part of the disturbance area intersects or overlaps the search area.

* **considerObservationAccuracy = false** and **considerDisturbanceRadius = false**.
  The search is performed solely against the observation centroid (`location.point`). Only observations whose point lies within the search geometry can result in a match.

### 6.3 Accuracy-based limitation

* If the **maxAccuracy** parameter is set, only observations with a coordinate uncertainty (`coordinateUncertaintyInMeters`) **less than or equal to** the specified value are included.

This limitation applies regardless of which geographic representation is otherwise used in the search.

---

## 7. Logging and traceability

All signal searches are logged to ensure:

* Traceability
* Usage monitoring
* Security auditing

---

## 8. Signal Search API endpoint

### 8.1 Endpoint

```
Observations_SignalSearchInternal
```

### 8.2 Headers

| Header                                 | Description                             |
| -------------------------------------- | --------------------------------------- |
| X-Authorization-Role-Id                | Limits authorization to a specific role |
| X-Authorization-Application-Identifier | Application used for authorization      |

### 8.3 Query parameters

| Parameter                      | Description                                  | Default |
| ------------------------------ | -------------------------------------------- | ------- |
| areaBuffer                     | Buffer around area (0–100 m)                 | 0       |
| onlyAboveMyClearance           | Signal only above user’s clearance           | true    |
| returnHttp4xxWhenNoPermissions | Return 403/409 on insufficient authorization | false   |

### 8.4 Response

| HTTP status      | Meaning                              |
| ---------------- | ------------------------------------ |
| 200 OK           | Returns `true` or `false`            |
| 400 Bad Request  | Invalid search parameters            |
| 401 Unauthorized | Authentication missing               |
| 403 Forbidden    | No authorization for the region      |
| 409 Conflict     | Partial authorization for the region |
