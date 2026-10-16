import ConfirmDeleteButton from "@/components/ui/ConfirmDeleteButton";
import FormDialog from "@/components/ui/FormDialog";
import { removeSupplierItem, saveMaterial, saveTool } from "../actions/supplierActions";
import { CRITICALITY_LABELS } from "../labels";
import type { SupplierDetail } from "../types";
import { CriticalityChip } from "./SupplierBadges";
import ItemDocuments from "./ItemDocuments";
import { MaterialFields, ToolFields } from "./ItemFields";
import { Empty, ItemBox, ItemList, Section } from "./Parts";

type Props = { supplier: SupplierDetail };

export function ToolsSection({ supplier }: Props) {
  const { id, canEdit } = supplier;

  return (
    <Section
      title="Ferramentais"
      count={supplier.tools.length}
      action={
        canEdit && (
          <FormDialog compact variant="outlined" title="Adicionar ferramental" trigger="Adicionar" action={saveTool.bind(null, id, null)}>
            <ToolFields />
          </FormDialog>
        )
      }
    >
      {supplier.tools.length === 0 ? (
        <Empty>Nenhum ferramental cadastrado.</Empty>
      ) : (
        <ItemList>
          {supplier.tools.map((tool) => (
            <ItemBox
              key={tool.id}
              title={tool.name}
              meta={tool.assetTag ? `Ativo fixo ${tool.assetTag}` : "Sem ativo fixo informado"}
              actions={
                canEdit && (
                  <>
                    <FormDialog compact title="Editar ferramental" trigger="Editar" action={saveTool.bind(null, id, tool.id)}>
                      <ToolFields tool={tool} />
                    </FormDialog>
                    <ConfirmDeleteButton name={tool.name} action={removeSupplierItem.bind(null, id, "tools", tool.id)} />
                  </>
                )
              }
            >
              <ItemDocuments supplierId={id} documents={supplier.documents} kinds={["toolDrawing"]} ownerId={tool.id} canEdit={canEdit} title="Desenhos" trigger="Anexar desenho" />
            </ItemBox>
          ))}
        </ItemList>
      )}
    </Section>
  );
}

export function MaterialsSection({ supplier }: Props) {
  const { id, canEdit } = supplier;

  return (
    <Section
      title="Materiais"
      count={supplier.materials.length}
      action={
        canEdit && (
          <FormDialog compact variant="outlined" title="Adicionar material" trigger="Adicionar" action={saveMaterial.bind(null, id, null)}>
            <MaterialFields />
          </FormDialog>
        )
      }
    >
      {supplier.materials.length === 0 ? (
        <Empty>Nenhum material cadastrado.</Empty>
      ) : (
        <ItemList>
          {supplier.materials.map((material) => (
            <ItemBox
              key={material.id}
              title={
                <>
                  {material.sku} <CriticalityChip criticality={material.criticality} />
                </>
              }
              meta={[
                material.businessUnit && `BU ${material.businessUnit}`,
                material.materialType,
                material.criticality && `Criticidade ${CRITICALITY_LABELS[material.criticality].toLowerCase()}`,
                material.hasSapSpecification === null ? null : material.hasSapSpecification ? "Com especificação no SAP" : "Sem especificação no SAP",
              ]
                .filter(Boolean)
                .join(" · ") || "Sem detalhes informados"}
              actions={
                canEdit && (
                  <>
                    <FormDialog compact title="Editar material" trigger="Editar" action={saveMaterial.bind(null, id, material.id)}>
                      <MaterialFields material={material} />
                    </FormDialog>
                    <ConfirmDeleteButton name={material.sku} action={removeSupplierItem.bind(null, id, "materials", material.id)} />
                  </>
                )
              }
            >
              <ItemDocuments supplierId={id} documents={supplier.documents} kinds={["materialDocument"]} ownerId={material.id} canEdit={canEdit} title="Documentos" />
            </ItemBox>
          ))}
        </ItemList>
      )}
    </Section>
  );
}

export function CompanyDocumentsSection({ supplier }: Props) {
  const { id, canEdit, documents } = supplier;

  return (
    <Section title="Documentos do fornecedor">
      <ItemDocuments supplierId={id} documents={documents} kinds={["presentation"]} canEdit={canEdit} title="Apresentação da empresa" trigger="Enviar apresentação" />
      <ItemDocuments supplierId={id} documents={documents} kinds={["certificate"]} canEdit={canEdit} title="Certificados" trigger="Anexar certificado" />
    </Section>
  );
}
